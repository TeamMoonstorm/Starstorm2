using SS2;
using RoR2;
using RoR2.ContentManagement;
using MSU;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2.Skills;
using R2API;
using RoR2.Projectile;

namespace SS2.Survivors
{
    public class Mage : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acMage", SS2Bundle.Vanilla);


        public static GameObject shatterEffectPrefab;
        private static float shatterDamageCoefficient = 0.5f;
        private static float shatterProcCoefficient = 1f;
        private static float shatterRadius = 8f;

        private static R2API.DamageAPI.ModdedDamageType PetrifyOnHit;
        private static R2API.ModdedProcType Shatter;
        public override void Initialize()
        {
            GameObject railgunnerBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Mage/MageBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = railgunnerBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily utilitySkillFamility = skillLocator.utility.skillFamily;

            SkillDef railGunnerRoll = SS2Assets.LoadAsset<SkillDef>("EarthSpear", SS2Bundle.Vanilla);

            AddSkill(utilitySkillFamility, railGunnerRoll);

            shatterEffectPrefab = SS2Assets.LoadAsset<GameObject>("ShatterEffect", SS2Bundle.Vanilla);

            

            PetrifyOnHit = R2API.DamageAPI.ReserveDamageType();
            Shatter = R2API.ProcTypeAPI.ReserveProcType();
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;

            var projectileDamage = SS2Assets.LoadAsset<GameObject>("MageEarthSpearProjectile", SS2Bundle.Vanilla)?.GetComponent<ProjectileDamage>();
            projectileDamage?.damageType.AddModdedDamageType(PetrifyOnHit);
        }

        private void OnServerDamageDealt(DamageReport damageReport)
        {
            var damageInfo = damageReport.damageInfo;
            var victimBody = damageReport.victimBody;
            var attackerBody = damageReport.attackerBody;

            {
                if (damageInfo.HasModdedDamageType(PetrifyOnHit))
                {
                    if (damageReport.victim && damageReport.victim.TryGetComponent(out SetStateOnHurt setStateOnHurt))
                    {
                        if (setStateOnHurt.canBeFrozen)
                        {
                            // TODO: Custom daataa!!!!!!!!!!!!!
                            setStateOnHurt.SetCustomState(EntityStateCatalog.GetStateIndex(typeof(EntityStates.PetrifiedState)), EntityStates.InterruptPriority.Frozen);
                        }
                    }
                }
            }

            if (!victimBody || !attackerBody)
            {
                return;
            }

            bool inPetrifiedState = false;
            if (victimBody.HasBuff(SS2Content.Buffs.bdPetrified)) // ??
            {
                inPetrifiedState = true;
            }

            if (inPetrifiedState && damageInfo.damage >= attackerBody.damage * 4f && !damageInfo.HasModdedDamageType(PetrifyOnHit) && !damageInfo.procChainMask.HasModdedProc(Shatter))
            {
                Vector3 position = victimBody.corePosition;
                float radius = shatterRadius + victimBody.radius;
                ProcChainMask procChainMask = damageInfo.procChainMask;
                procChainMask.AddModdedProc(Shatter);

                if (shatterEffectPrefab)
                {
                    EffectManager.SpawnEffect(shatterEffectPrefab, new EffectData
                    {
                        origin = position,
                        scale = radius,
                    }, true);
                }

                new BlastAttack
                {
                    attacker = damageReport.attacker,
                    inflictor = damageReport.damageInfo.inflictor,
                    attackerFiltering = AttackerFiltering.Default,
                    position = position,
                    teamIndex = damageReport.attackerTeamIndex,
                    radius = radius,
                    baseDamage = damageInfo.damage * shatterDamageCoefficient,
                    damageType = DamageType.Stun1s,
                    crit = damageInfo.crit,
                    procCoefficient = shatterProcCoefficient,
                    procChainMask = procChainMask,
                    baseForce = 600f,
                    damageColorIndex = DamageColorIndex.WeakPoint,
                    falloffModel = BlastAttack.FalloffModel.Linear,
                    losType = BlastAttack.LoSType.NearestHit,
                }.Fire();

                if (victimBody.HasBuff(SS2Content.Buffs.bdPetrified)) /////////////
                {
                    victimBody.RemoveBuff(SS2Content.Buffs.bdPetrified);
                }
            }
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack) => SS2Config.enableBeta;
    }
}
