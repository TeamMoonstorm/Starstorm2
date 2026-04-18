using MSU;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using RoR2.ContentManagement;
using R2API;
using EntityStates;
using MSU.Config;
using RoR2.Skills;

namespace SS2.Survivors
{
    // TODO:  When I am back in Unityland to bump parryBuffDuration from 0.4s to 1.0s+ in escKnightShield.asset
    public sealed class Knight : SS2Survivor
    {
        /// <summary>
        /// How long (in seconds) the "before" parry window remembers the last attacker
        /// When the player presses secondary, Shield.OnEnter checks if an attack landed within
        /// this window and triggers a retroactive parry (visual feedback + buffed skills)
        /// We want to reward players whose timing was slightly late by still granting the parry payoff
        /// and where network latency might have caused issues too
        /// This wont retroactively reject damage though
        /// </summary>
        public static float lastAttackWindow = 0.5f;
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acKnight", SS2Bundle.Indev);
        
        public static GameObject KnightImpactEffect;
        public static GameObject KnightCrosshair;
        public static GameObject KnightDroppod;
        public static GameObject KnightPassiveWard;
        public static GameObject KnightHitEffect;
        public static GameObject KnightSpinEffect;
        public static GameObject smiteEffectPrefab;

        public static SerializableEntityStateType ShieldStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Knight.Shield));

        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        public static Vector3 chargeCameraPos = new Vector3(1.2f, -0.25f, -6.1f);
        public static Vector3 altCameraPos = new Vector3(-1.2f, -0.25f, -6.1f);

        public static float smiteHealthFractionRestoration = 0.02f;
        public static float smiteArmorReduction = 3f;

        
        public static CharacterCameraParamsData chargeCameraParams = new CharacterCameraParamsData
        {
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = chargeCameraPos,
            wallCushion = 0.1f,
        };

        public static CharacterCameraParamsData altCameraParams = new CharacterCameraParamsData
        {
            pivotVerticalOffset = 1f,
            idealLocalCameraPos = altCameraPos,
            wallCushion = 0.1f,
        };

        public AssetCollection ExtraAssetCollection { get; set; }

        public static float reducedGravity = 0.07f;
        public static DamageAPI.ModdedDamageType ExtendedStunDamageType { get; set; }
        private static ModdedProcType Smite;

        private static float extendedStunDuration = 2f;
        public override void Initialize()
        {
            KnightHitEffect = AssetCollection.FindAsset<GameObject>("KnightImpactSlashEffect");
            KnightSpinEffect = AssetCollection.FindAsset<GameObject>("KnightSpin");
            KnightPassiveWard = AssetCollection.FindAsset<GameObject>("KnightPassiveBuffWard");
            smiteEffectPrefab = AssetCollection.FindAsset<GameObject>("KnightSmiteEffect");

            AssetCollection.FindAsset<UpgradedSkillDef>("sdKnightBuffedPrimaryLunar").upgradedFrom = Addressables.LoadAssetAsync<LunarPrimaryReplacementSkill>("RoR2/Base/LunarSkillReplacements/LunarPrimaryReplacement.asset").WaitForCompletion();
            //AssetCollection.FindAsset<UpgradedSkillDef>("sdKnightBuffedUtilityLunar").upgradedFrom = Addressables.LoadAssetAsync<SkillDef>("RoR2/Base/LunarSkillReplacements/LunarUtilityReplacement.asset").WaitForCompletion();
            //AssetCollection.FindAsset<UpgradedSkillDef>("sdKnightBuffedSpecialLunar").upgradedFrom = Addressables.LoadAssetAsync<LunarDetonatorSkill>("RoR2/Base/LunarSkillReplacements/LunarDetonatorSpecialReplacement.asset").WaitForCompletion();

            KnightImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2SmokeBomb.prefab").WaitForCompletion();
            KnightCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/SimpleDotCrosshair.prefab").WaitForCompletion();
            KnightDroppod = Addressables.LoadAssetAsync<GameObject>("Prefabs/NetworkedObjects/SurvivorPod").WaitForCompletion();

            ExtendedStunDamageType = R2API.DamageAPI.ReserveDamageType();
            Smite = R2API.ProcTypeAPI.ReserveProcType();
            ModifyPrefab();
            R2API.RecalculateStatsAPI.GetStatCoefficients += ModifyStats;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;

            // Add the buff material overlays to buffoverlay dict
            BuffDef buffKnightSpecialPower = AssetCollection.FindAsset<BuffDef>("bdKnightSpecialPowerBuff");
            Material matSpecialPowerOverlay = AssetCollection.FindAsset<Material>("matKnightBuffOverlay");
            BuffOverlays.AddBuffOverlay(buffKnightSpecialPower, matSpecialPowerOverlay);
        }

        private void OnServerDamageDealt(DamageReport damageReport)
        {
            if (DamageAPI.HasModdedDamageType(damageReport.damageInfo, ExtendedStunDamageType))
            {
                if (damageReport.victimBody.TryGetComponent(out SetStateOnHurt setStateOnHurt) && setStateOnHurt.canBeStunned)
                {
                    setStateOnHurt.SetStun(extendedStunDuration);
                }
            }

            if (damageReport.damageInfo.procCoefficient > 0 && damageReport.attackerBody && damageReport.attackerBody.HasBuff(SS2Content.Buffs.bdKnightSpecialPowerBuff) && !damageReport.damageInfo.procChainMask.HasModdedProc(Smite))
            {
                damageReport.damageInfo.procChainMask.AddModdedProc(Smite);
                damageReport.attackerBody.healthComponent.HealFraction(smiteHealthFractionRestoration * damageReport.damageInfo.procCoefficient, default(ProcChainMask));
                damageReport.victimBody.AddBuff(SS2Content.Buffs.bdKnightSpecialDebuff);

                EffectManager.SimpleImpactEffect(smiteEffectPrefab, damageReport.damageInfo.position, Vector3.zero, true);
            }
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = KnightCrosshair;
            cb.preferredPodPrefab = KnightDroppod;

            EntityStateMachine bodyState = EntityStateMachine.FindByCustomName(cb.gameObject, "Body");
            bodyState.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Knight.MainState));
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta && base.IsAvailable(contentPack);
        }

        private void ModifyStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            // The buff behavior for Knight's default passive
            if (sender.HasBuff(SS2Content.Buffs.bdKnightBuff))
            {
                args.attackSpeedMultAdd += 0.2f;
                args.damageMultAdd += 0.2f;
            }

            if (sender.HasBuff(SS2Content.Buffs.bdKnightShield))
            {
                args.armorAdd += 200f;
            }

            int debuffCount = sender.GetBuffCount(SS2Content.Buffs.bdKnightSpecialDebuff);
            if (debuffCount > 0)
            {
                args.armorAdd -= debuffCount * smiteArmorReduction;
            }
        }
    }
}
