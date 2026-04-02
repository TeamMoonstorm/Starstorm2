using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class LowQualitySpeakers : SS2Item
    {
        private const string token = "SS2_ITEM_LOWQUALITYSPEAKERS_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acLowQualitySpeakers", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius in which enemies are stunned, in meters.")]
        [FormatToken(token, 0)]
        public static float baseRadius = 12f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Additional stun radius per stack.")]
        [FormatToken(token, 1)]
        public static float radiusPerStack = 6f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for this item to proc on taking damage")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float baseProcChance = 0.1f;

        public static float maxProcChance = 1f;

        public static float cooldown = 1f;
        public static float knockbackForce = 14f;
        public static float knockUpForce = 14f;
        private static GameObject _burstEffect;

        public override void Initialize()
        {
            _burstEffect = AssetCollection.FindAsset<GameObject>("SpeakerBurstEffect");
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.LowQualitySpeakers;

            private float cooldownStopwatch;
            private bool destroyProjectilesNextFrame; // projectiles can damage the player, proc LQS, THEN spawn children, which wont get deleted. destroy projectiles next frame instead.

            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    if (destroyProjectilesNextFrame)
                    {
                        destroyProjectilesNextFrame = false;
                        CleanseSystem.RemoveNearbyProjectilesServer(body.teamComponent.teamIndex, body.corePosition, baseRadius + stack * (radiusPerStack - 1));
                    }

                    cooldownStopwatch -= Time.fixedDeltaTime;
                }
                
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                int stack = body.inventory.GetItemCountEffective(SS2Content.Items.LowQualitySpeakers);

                if (stack <= 0 || cooldownStopwatch > 0) return;
                cooldownStopwatch = cooldown;

                // maxProcChance chance at 0% health, baseProcChance at 100% health
                // uses health fraction after damage, not before
                float procChance = Mathf.Lerp(maxProcChance, baseProcChance, body.healthComponent.combinedHealthFraction);
                if (Util.CheckRoll(procChance * 100f, body.master))
                {
                    float radius = baseRadius + stack * (radiusPerStack - 1);

                    BlastAttack blastAttack = new BlastAttack();
                    blastAttack.position = body.corePosition;
                    blastAttack.baseDamage = 0f;
                    blastAttack.baseForce = knockbackForce;
                    blastAttack.bonusForce = Vector3.up * knockUpForce;
                    blastAttack.physForceFlags = PhysForceFlags.ignoreGroundStick | PhysForceFlags.massIsOne | PhysForceFlags.resetVelocity;
                    blastAttack.radius = radius;
                    blastAttack.attacker = body.gameObject;
                    blastAttack.inflictor = body.gameObject;
                    blastAttack.teamIndex = body.teamComponent.teamIndex;
                    blastAttack.crit = false;
                    blastAttack.procChainMask = default(ProcChainMask);
                    blastAttack.procCoefficient = 1f;
                    blastAttack.falloffModel = BlastAttack.FalloffModel.Linear;
                    blastAttack.damageColorIndex = DamageColorIndex.Default;
                    blastAttack.damageType = DamageType.Stun1s;
                    blastAttack.attackerFiltering = AttackerFiltering.NeverHitSelf;
                    blastAttack.Fire();

                    destroyProjectilesNextFrame = true;
                    

                    EffectManager.SpawnEffect(_burstEffect, new EffectData
                    {
                        origin = body.corePosition,
                        scale = radius,
                        rotation = Quaternion.identity
                    }, true);

                }
            }
        }
    }
}
