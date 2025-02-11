using RoR2;
using UnityEngine;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using MSU;
using SS2;
using RoR2.Skills;
using EntityStates;
using System.Collections;
using RoR2.ContentManagement;
using R2API;
//#if DEBUG
namespace SS2.Survivors
{
    public sealed class NemCaptain : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acNemCaptain", SS2Bundle.Indev);

        public static BuffDef _buffDefOverstress;
        public static BuffDef _buffDefTotalReset;
        //public static BuffDef _buffDefTacticalDecisionMaking;

        public override void Initialize()
        {
            _buffDefOverstress = AssetCollection.FindAsset<BuffDef>("bdOverstress");
            _buffDefTotalReset = AssetCollection.FindAsset<BuffDef>("bdTotalReset");
            //_buffDefTacticalDecisionMaking = AssetCollection.FindAsset<BuffDef>("bdTacticalDecisionMaking");

            //Temporary but looks kinda cool ngl

            //_buffDefTacticalDecisionMaking.iconSprite = Addressables.LoadAssetAsync<BuffDef>("RoR2/DLC2/bdDisableAllSkills.asset").WaitForCompletion().iconSprite;
            //_buffDefTacticalDecisionMaking.buffColor = new Color(0.2f, 0.6f, 0.2f);

            ModifyPrefab();
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.GenericSkill.RecalculateMaxStock += (orig, self) =>
            {
                if (self.skillDef != null && self.skillDef is OrderSkillDef orderSkillDef)
                {
                    self.maxStock = (int)orderSkillDef.stressValue;
                }
                else
                    orig(self);
            };
            On.EntityStates.SkillStateOverrideData.InternalOverride += SkillStateOverrideData_InternalOverride;
        }

        private void SkillStateOverrideData_InternalOverride(On.EntityStates.SkillStateOverrideData.orig_InternalOverride orig, SkillStateOverrideData self, GenericSkill existingSkillToOverride, ref SkillDef overridingSkill, ref GenericSkill originalSkill, ref int previousStockAmount)
        {
            //orig(self, existingSkillToOverride, ref overridingSkill, ref originalSkill, ref previousStockAmount);
            if (!(overridingSkill == null) && !(existingSkillToOverride == null) && !(existingSkillToOverride == originalSkill) && !existingSkillToOverride.HasSkillOverrideOfPriority(SkillStateOverrideData.priority))
            {
                int stock = existingSkillToOverride.stock;
                float rechargeStopwatch = existingSkillToOverride.rechargeStopwatch;
                previousStockAmount = -1;
                originalSkill = existingSkillToOverride;
                originalSkill.SetSkillOverride(self.source, overridingSkill, SkillStateOverrideData.priority);
                if (self.duplicateStock)
                {
                    originalSkill.stock = stock;
                    originalSkill.rechargeStopwatch = rechargeStopwatch;
                }
            }
        }

        public sealed class OverstressBuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _buffDefOverstress;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (hasAnyStacks)
                {
                    args.armorAdd -= 20f;
                    args.moveSpeedReductionMultAdd += 0.3f;
                    args.damageMultAdd -= 0.5f;
                }
            }
        }

        public sealed class TotalResetBuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _buffDefTotalReset;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (hasAnyStacks)
                {
                    args.moveSpeedMultAdd += 0.25f;
                }
            }
        }

        /*public sealed class TacticalDecisionMakingBuffBehavior : BaseBuffBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => _buffDefTacticalDecisionMaking;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
            }
        }*/

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(SS2Content.Buffs.bdNemCapDroneBuff))
            {
                args.armorAdd += 30f;
                args.baseAttackSpeedAdd += 0.2f;
            }
        }

        public void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            //cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
//#endif