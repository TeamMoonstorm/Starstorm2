using RoR2;
using RoR2.Items;
using UnityEngine;
using System.Collections.Generic;

using Moonstorm;
namespace SS2.Items
{

    public sealed class CrypticSource : ItemBase
    {
        private const string token = "SS2_ITEM_CRYPTICSOURCE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("CrypticSource", SS2Bundle.Items);

        public static GameObject readyEffectPrefab { get; } = SS2Assets.LoadAsset<GameObject>("CrypticSourceReady", SS2Bundle.Items);
        public static GameObject explosionEffectPrefab { get; } = SS2Assets.LoadAsset<GameObject>("CrypticSourceExplosion", SS2Bundle.Items);
       
        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Radius of the energy burst, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float baseRadius = 15f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Radius of the energy burst per stack, in meters.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float stackRadius = 3f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Base damage of the energy burst, per stack. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float damageCoefficient = 2.5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Proc coefficient of the energy burst.")]
        public static float procCoefficient = 1f;

        public static float minimumSprintDistance = 12.5f;
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
            }
            private void FixedUpdate()
            {
                bool isSprinting = base.body.isSprinting;
                Vector3 currentPosition = base.transform.position;
                if(wasSprinting && !isSprinting)
                {
                    // do burst                   
                    if(this.attackReady)
                    {
                        this.attackReady = false;
                        if (this.readyEffect) Destroy(this.readyEffect);
                        AkSoundEngine.StopPlayingID(soundId);
                        this.Fire();                        
                    }
                    this.distanceTravelledThisSprint = 0;
                }
                if(isSprinting)
                {
                    this.distanceTravelledThisSprint += Vector3.Distance(previousPosition, currentPosition);
                    // start effects
                    if (!this.attackReady && distanceTravelledThisSprint >= minimumSprintDistance)
                    {
                        this.readyEffect = GameObject.Instantiate(readyEffectPrefab, body.coreTransform);
                        this.soundId = Util.PlaySound("CrypticSourceStage1", base.gameObject);
                        this.attackReady = true;
                    }
                }

                previousPosition = currentPosition;
                wasSprinting = isSprinting;
            }

            private void Fire()
            {
                this.attackReady = false;

                float radius = baseRadius + stackRadius * (stack - 1);
                EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
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
