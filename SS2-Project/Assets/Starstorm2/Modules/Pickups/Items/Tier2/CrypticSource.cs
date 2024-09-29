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
    public sealed class CrypticSource : SS2Item
    {
        private const string token = "SS2_ITEM_CRYPTICSOURCE_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acCrypticSource", SS2Bundle.Items);

        private static GameObject _readyEffectPrefab;
        private static GameObject _explosionEffectPrefab;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius of the energy burst, in meters.")]
        [FormatToken(token, 0)]
        public static float baseRadius = 15f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Radius of the energy burst per stack, in meters.")]
        [FormatToken(token, 1)]
        public static float stackRadius = 3f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Base damage of the energy burst, per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float damageCoefficient = 2.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Proc coefficient of the energy burst.")]
        public static float procCoefficient = 1f;

        public static float minimumSprintDistance = 12.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Should Cryptic Source explode automatically after most dash skills.")]
        public static bool AutoBurst = true;
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void Initialize()
        {
            _readyEffectPrefab = AssetCollection.FindAsset<GameObject>("CrypticSourceReady");
            _explosionEffectPrefab = AssetCollection.FindAsset<GameObject>("CrypticSourceExplosion");
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.CrypticSource;

            private bool wasSprinting;
            private float distanceTravelledThisSprint;
            private bool attackReady;
            private GameObject readyEffect;
            private uint soundId;

            private EntityStateMachine bodyStateMachine;
            private Vector3 previousPosition; // cant just calculate from velocity because some states use rootmotion

            private void Start()
            {
                this.bodyStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Body");
                if(AutoBurst)
                    this.bodyStateMachine.nextStateModifier += TryBurst;
            }
            private void TryBurst(EntityStateMachine machine, ref EntityStates.EntityState nextState)
            {
                if (!machine.IsInMainState() && nextState.GetType() == machine.mainStateType.stateType && this.attackReady)
                {
                    DoBurst();
                }
            }
            private void FixedUpdate()
            {
                bool isSprinting = base.body.isSprinting;
                Vector3 currentPosition = base.transform.position;
                if (wasSprinting && !isSprinting)
                {
                    // do burst                   
                    if (this.attackReady)
                    {
                        DoBurst();
                    }
                    this.distanceTravelledThisSprint = 0;
                }
                if (isSprinting)
                {
                    this.distanceTravelledThisSprint += Vector3.Distance(previousPosition, currentPosition);
                    // start effects
                    if (!this.attackReady && distanceTravelledThisSprint >= minimumSprintDistance)
                    {
                        this.readyEffect = GameObject.Instantiate(_readyEffectPrefab, body.coreTransform);
                        this.soundId = Util.PlaySound("CrypticSourceStage1", base.gameObject);
                        this.attackReady = true;
                    }
                }

                previousPosition = currentPosition;
                wasSprinting = isSprinting;
            }

            private void DoBurst()
            {
                this.distanceTravelledThisSprint = 0;
                this.attackReady = false;
                if (this.readyEffect) Destroy(this.readyEffect);
                AkSoundEngine.StopPlayingID(soundId);
                this.Fire();
            }
            private void Fire()
            {
                this.attackReady = false;
                if (!NetworkServer.active) return;

                float radius = baseRadius + stackRadius * (stack - 1);
                EffectManager.SpawnEffect(_explosionEffectPrefab, new EffectData
                {
                    origin = body.corePosition,
                    scale = radius,
                }, true);
                new BlastAttack
                {
                    attacker = body.gameObject,
                    inflictor = body.gameObject,
                    attackerFiltering = AttackerFiltering.NeverHitSelf,
                    position = body.corePosition,
                    teamIndex = body.teamComponent.teamIndex,
                    radius = radius,
                    baseDamage = body.damage * damageCoefficient * stack,
                    damageType = DamageType.Generic,
                    crit = body.RollCrit(),
                    procCoefficient = procCoefficient,
                    procChainMask = default(ProcChainMask),
                    baseForce = 0f,
                    damageColorIndex = DamageColorIndex.Item,
                    falloffModel = BlastAttack.FalloffModel.None,
                    losType = BlastAttack.LoSType.None,
                }.Fire();
            }

            private void OnDestroy()
            {
                if (this.attackReady) AkSoundEngine.StopPlayingID(soundId);
                if (this.readyEffect) Destroy(this.readyEffect);
            }
        }
    }
}
