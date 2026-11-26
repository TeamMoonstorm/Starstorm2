using System;
using System.Collections.Generic;
using RoR2;
using RoR2.Orbs;
using SS2;
using UnityEngine;
using UnityEngine.Networking;
using static EntityStates.AffixStorm.DeathState;

namespace EntityStates.BallLightning
{
    public class BallLightningBaseState : BaseState
    {
        protected NetworkedBodyAttachment networkedBodyAttachment { get; private set; }

        protected HealthComponent attachedHealthComponent { get; private set; }

        protected CharacterModel attachedCharacterModel { get; private set; }

        public override void OnEnter()
        {
            base.OnEnter();

            networkedBodyAttachment = GetComponent<NetworkedBodyAttachment>();
            if (networkedBodyAttachment && networkedBodyAttachment.attachedBody)
            {
                attachedHealthComponent = networkedBodyAttachment.attachedBody.healthComponent;
                if (networkedBodyAttachment.attachedBody.modelLocator)
                {
                    Transform modelTransform = networkedBodyAttachment.attachedBody.modelLocator.modelTransform;
                    if (modelTransform)
                    {
                        attachedCharacterModel = modelTransform.GetComponent<CharacterModel>();
                    }
                }
            }
        }
    }

    public class CountDown : BallLightningBaseState
    {
        private static float duration = 15f;

        private static float warningDuration = 3f;
        private static float healthFractionDamage = 0.75f;
        private static float orbDuration = 0.5f;
        private static float orbBlastRadius = 3f;
        private static string warningSoundString = "Play_item_use_BFG_charge";
        public static GameObject effectPrefab;
        public static GameObject explosionEffectPrefab;
        public static GameObject orbEffectPrefab;
        public static GameObject orbImpactEffectPrefab;

        private GameObject effectInstance;

        private bool warned = false;
        private bool detonated = false;

        public override void OnEnter()
        {
            base.OnEnter();

            if (effectPrefab && attachedCharacterModel)
            {
                effectInstance = GameObject.Instantiate<GameObject>(effectPrefab, attachedHealthComponent.body.corePosition, Quaternion.identity);
                effectInstance.transform.localScale = Vector3.one * Mathf.Min(attachedHealthComponent.body.radius, 1f);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= duration - warningDuration && !warned)
            {
                warned = true;
                Util.PlaySound(warningSoundString, gameObject);

                if (effectInstance)
                {
                    var warningTransform = effectInstance.transform.Find("DetonationWarning");
                    if (warningTransform)
                    {
                        warningTransform.gameObject.SetActive(true);
                    }
                }
            }

            if (NetworkServer.active)
            {
                if (fixedAge >= duration && !detonated)
                {
                    detonated = true;
                    Detonate();
                }
            }
            
        }
        public override void Update()
        {
            base.Update();

            if (effectInstance && networkedBodyAttachment.attachedBody)
            {
                effectInstance.transform.position = networkedBodyAttachment.attachedBody.corePosition;
            }
        }

        public override void OnExit()
        {
            if (effectInstance)
            {
                Destroy(effectInstance);
            }

            base.OnExit();
        }
        public void Detonate()
        {
            if (networkedBodyAttachment.attachedBody)
            {
                EffectManager.SpawnEffect(explosionEffectPrefab, new EffectData
                {
                    origin = networkedBodyAttachment.attachedBody.corePosition,
                }, true);

                float damage = attachedHealthComponent.fullHealth * healthFractionDamage;
                OrbManager.instance.AddOrb(new AffixStormStrikeOrb
                {
                    attacker = networkedBodyAttachment.attachedBody.gameObject,
                    damageColorIndex = DamageColorIndex.Electrocution,
                    teamIndex = networkedBodyAttachment.attachedBody.teamComponent.teamIndex,
                    damageValue = damage,
                    isCrit = false,
                    procChainMask = default(ProcChainMask),
                    damageType = DamageType.Shock5s,
                    procCoefficient = 0f,
                    target = networkedBodyAttachment.attachedBody.mainHurtBox,
                });

                networkedBodyAttachment.attachedBody.inventory.SetEquipmentIndex(EquipmentIndex.None);

                outer.SetNextState(new Idle());
            }

        }

        public class SelfDestructOrb : GenericDamageOrb, IOrbFixedUpdateBehavior
        {
            private Vector3 lastKnownTargetPosition;
            public GameObject inflictor;
            public override void Begin()
            {
                base.Begin();
                base.duration = orbDuration;
            }

            public void FixedUpdate()
            {
                if (this.target)
                {
                    this.lastKnownTargetPosition = this.target.transform.position;
                }
            }

            public override GameObject GetOrbEffect()
            {
                return orbEffectPrefab ?? LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect");
            }

            public override void OnArrival()
            {
                var effectPrefab = orbImpactEffectPrefab ?? LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/LightningStrikeImpact");
                EffectManager.SpawnEffect(effectPrefab, new EffectData
                {
                    origin = this.lastKnownTargetPosition
                }, true);

                new BlastAttack
                {
                    attacker = this.attacker,
                    baseDamage = this.damageValue,
                    baseForce = 0f,
                    bonusForce = Vector3.down * 3000f,
                    crit = this.isCrit,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Shock5s,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    inflictor = this.inflictor,
                    position = this.lastKnownTargetPosition,
                    procChainMask = this.procChainMask,
                    procCoefficient = 0f,
                    radius = orbBlastRadius,
                    teamIndex = this.teamIndex,
                    attackerFiltering = AttackerFiltering.AlwaysHit,
                }.Fire();

            }

        }

    }

}
