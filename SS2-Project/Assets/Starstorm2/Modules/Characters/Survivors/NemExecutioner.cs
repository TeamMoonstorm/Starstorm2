using Mono.Cecil.Cil;
using MonoMod.Cil;
using SS2.Components;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using R2API;
using RoR2.ContentManagement;
using RoR2.Orbs;
using R2API.Utils;
using MSU;

namespace SS2.Survivors
{
    public sealed class NemExecutioner : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemExecutioner", SS2Bundle.NemExecutioner);

        public static DamageAPI.ModdedDamageType fearOnHit;
        public static DamageAPI.ModdedDamageType ricochetOnHit;
        public static DamageAPI.ModdedDamageType healNovaOnKill;

        
        public override void Initialize()
        {
            ModifyPrefab();

            fearOnHit = DamageAPI.ReserveDamageType(); // A lot of fear code is in Executioner2. i dont rly like this
            ricochetOnHit = DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            GlobalEventManager.onCharacterDeathGlobal += OnCharacterDeathGlobal;

            orbEffectPrefab = SS2Assets.LoadAsset<GameObject>("RicochetBallOrbEffect", SS2Bundle.NemExecutioner);
            fearEffectPrefab = SS2Assets.LoadAsset<GameObject>("FearEffectRed", SS2Bundle.NemExecutioner);
        }


        private static float healNovaRadius = 16f;
        private static float healNovaPercentHeal = 0.2f;
        private static float healNovaDamageCoefficient = 0.1f;
        private static GameObject healNovaEffectPrefab;
        private void OnCharacterDeathGlobal(DamageReport damageReport)
        {
            if (damageReport.damageInfo.HasModdedDamageType(healNovaOnKill))
            {
                EffectData data = new EffectData
                {
                    origin = damageReport.damageInfo.position,
                    scale = healNovaRadius,
                };
                EffectManager.SpawnEffect(healNovaEffectPrefab, data, true);

                SphereSearch search = new SphereSearch();
                search.mask = LayerIndex.entityPrecise.mask;
                search.radius = healNovaRadius;
                search.origin = damageReport.damageInfo.position;
                TeamMask mask = TeamMask.none;
                mask.AddTeam(damageReport.attackerTeamIndex);
                search.FilterCandidatesByHurtBoxTeam(mask);
                search.FilterCandidatesByDistinctHurtBoxEntities();
                foreach (HurtBox hurtBox in search.GetHurtBoxes())
                {
                    if (hurtBox && hurtBox.healthComponent)
                    {
                        float amount = healNovaPercentHeal * hurtBox.healthComponent.fullHealth + healNovaDamageCoefficient * damageReport.damageDealt;
                        hurtBox.healthComponent.Heal(amount, default(ProcChainMask));
                    }
                }
            }
        }


        private static float orbDuration = 0.125f;
        private static int orbBounces = 2;
        private static float orbRange = 12f;
        private static float orbDamageCoefficient = 1f;
        private static float orbProcCoefficient = 1f;
        public static GameObject orbEffectPrefab;
        private void OnServerDamageDealt(DamageReport damageReport)
        {
            if(damageReport.damageInfo.HasModdedDamageType(fearOnHit))
            {
                damageReport.victimBody.AddBuff(SS2Content.Buffs.BuffFearRed);
                // TODO: unify fear code
            }
            if (damageReport.damageInfo.HasModdedDamageType(ricochetOnHit))
            {
                CharacterBody body = damageReport.attackerBody;

                CustomLightningOrb orb = new CustomLightningOrb();
                orb.orbEffectPrefab = orbEffectPrefab;

                orb.duration = orbDuration;
                orb.bouncesRemaining = orbBounces - 1;
                orb.range = orbRange;
                orb.damageCoefficientPerBounce = orbProcCoefficient;
                orb.damageValue = damageReport.damageInfo.damage * orbDamageCoefficient;
                orb.damageType = DamageType.Generic;
                orb.isCrit = damageReport.damageInfo.crit;
                orb.damageColorIndex = DamageColorIndex.Default;
                orb.procCoefficient = orbProcCoefficient;
                orb.origin = damageReport.victimBody.corePosition;
                orb.teamIndex = body.teamComponent.teamIndex;
                orb.attacker = body.gameObject;
                orb.procChainMask = default(ProcChainMask);
                orb.bouncedObjects = new List<HealthComponent>();
                HurtBox hurtbox = orb.PickNextTarget(damageReport.victimBody.corePosition, damageReport.victim);
                if (hurtbox)
                {
                    orb.target = hurtbox;
                    OrbManager.instance.AddOrb(orb);
                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public void ModifyPrefab()
        {
            SetupDefaultBody(CharacterPrefab);
        }


        public static GameObject fearEffectPrefab;
        public sealed class FearBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffFear;

            private GameObject effectInstance;
            private static string activationSoundString = "Play_voidman_R_pop";
            private Collider bodyCollider;
            private int previousBuffCount;
            private void OnEnable()
            {
                previousBuffCount = buffCount;
                OnStackGained();
                if (!bodyCollider)
                    bodyCollider = base.GetComponent<Collider>();
            }
            private void OnDisable()
            {
                Destroy(effectInstance);
            }

            private void FixedUpdate()
            {
                if (buffCount > previousBuffCount)
                {
                    OnStackGained();
                }
                if (!base.characterBody.healthComponent.alive)
                    Destroy(this.effectInstance);
                previousBuffCount = buffCount;
            }

            private void OnStackGained()
            {
                if (effectInstance) Destroy(effectInstance);
                effectInstance = GameObject.Instantiate(fearEffectPrefab, characterBody.coreTransform.position, Quaternion.identity);
                Util.PlaySound(activationSoundString, gameObject);
            }

            private void Update()
            {
                if (effectInstance)
                {
                    Vector3 a = base.transform.position;
                    if (this.bodyCollider)
                    {
                        a = this.bodyCollider.bounds.center + new Vector3(0f, this.bodyCollider.bounds.extents.y, 0f);
                    }
                    effectInstance.transform.position = a;
                }
            }
        }
    }
}