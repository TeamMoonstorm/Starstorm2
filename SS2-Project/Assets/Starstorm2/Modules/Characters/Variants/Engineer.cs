using Assets.Starstorm2.ContentClasses;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using SS2;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using static R2API.DamageAPI;
using R2API;

namespace SS2.Survivors
{
    public class Engineer : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<AssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<AssetCollection>("acEngineer", SS2Bundle.Indev);

        public static ModdedDamageType EngiFocusDamage { get; private set; }
        public static ModdedDamageType EngiFocusDamageProc { get; private set; }
        public static BuffDef _buffDefEngiFocused;

        public override void Initialize()
        {
            //_buffDefEngiFocused = survivorAssetCollection.FindAsset<BuffDef>("bdEngiFocused");

            SkillDef sdLaserFocus = survivorAssetCollection.FindAsset<SkillDef>("sdLaserFocus");
            SkillDef sdRapidDisplacement = survivorAssetCollection.FindAsset<SkillDef>("sdRapidDisplacement");

            GameObject engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = engiBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamilyPrimary = skillLocator.primary.skillFamily;
            SkillFamily skillFamilyUtility = skillLocator.utility.skillFamily;

            Debug.Log("sdRD: " + sdRapidDisplacement);

            // If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the existing Skill Family.
            Array.Resize(ref skillFamilyPrimary.variants, skillFamilyPrimary.variants.Length + 1);
            skillFamilyPrimary.variants[skillFamilyPrimary.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdLaserFocus,
                viewableNode = new ViewablesCatalog.Node(sdLaserFocus.skillNameToken, false, null)
            };

            //Array.Resize(ref skillFamilyUtility.variants, skillFamilyUtility.variants.Length + 1);
            //skillFamilyUtility.variants[skillFamilyUtility.variants.Length - 1] = new SkillFamily.Variant
            //{
            //    skillDef = sdRapidDisplacement,
            //    viewableNode = new ViewablesCatalog.Node(sdRapidDisplacement.skillNameToken, false, null)
            //};

            EngiFocusDamage = DamageAPI.ReserveDamageType();
            EngiFocusDamageProc = DamageAPI.ReserveDamageType();

            On.RoR2.HealthComponent.TakeDamage += EngiFocusDamageHook;
        }

        private void EngiFocusDamageHook(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var dmg = damageInfo.HasModdedDamageType(EngiFocusDamage);
            var proc = damageInfo.HasModdedDamageType(EngiFocusDamageProc);
            if (dmg || proc)
            {
                int count = self.body.GetBuffCount(SS2Content.Buffs.bdEngiFocused);
                Debug.Log("count " + count);
                //Debug.Log("graaa can stack " + _buffDefEngiFocused.canStack + " |" + SS2Content.Buffs.bdEngiFocused.canStack);
                if (proc && count < 5)
                {
                    Debug.Log("adding buff ");
                    SS2Util.RefreshAllBuffStacks(self.body, SS2Content.Buffs.bdEngiFocused, 5);
                    self.body.AddTimedBuffAuthority(SS2Content.Buffs.bdEngiFocused.buffIndex, 5);

                }
                else if (proc)
                {
                    Debug.Log("refrsjhes ");
                    SS2Util.RefreshAllBuffStacks(self.body, SS2Content.Buffs.bdEngiFocused, 5);
                }

                if (count > 0)
                {
                    damageInfo.damage *= 1 + (count * .15f);
                }
            }
            orig(self, damageInfo);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class EngiFocusedBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdEngiFocused;
        }
    }
}
