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
            _buffDefEngiFocused = survivorAssetCollection.FindAsset<BuffDef>("bdEngiFocused");

            SkillDef sdLaserFocus = survivorAssetCollection.FindAsset<SkillDef>("sdLaserFocus");

            GameObject engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = engiBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            // If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the existing Skill Family.
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdLaserFocus,
                viewableNode = new ViewablesCatalog.Node(sdLaserFocus.skillNameToken, false, null)
            };

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
                if (proc && count < 5)
                {
                    Debug.Log("adding buff ");
                    self.body.AddTimedBuffAuthority(SS2Content.Buffs.bdEngiFocused.buffIndex, 5);
                }
                else if (proc)
                {
                    Debug.Log("refrsjhes ");
                    SS2Util.RefreshAllBuffStacks(self.body, SS2Content.Buffs.bdEngiFocused, 5);
                }

                if (count > 0)
                {
                    damageInfo.damage *= 1 + (count * .1f);
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

            //public void OnIncomingDamageOther(HealthComponent victimHealthComponent, DamageInfo damageInfo)
            //{
            //    Debug.Log("GRAHH " + EngiFocusDamage + " | " + SS2Content.Buffs.bdEngiFocused);
            //    if (damageInfo.HasModdedDamageType(EngiFocusDamage))
            //    {
            //        int count = victimHealthComponent.body.GetBuffCount(SS2Content.Buffs.bdEngiFocused);
            //        if (Util.CheckRoll(damageInfo.procCoefficient/2f) && count < 5)
            //        {
            //            Debug.Log("adding buff ");
            //            victimHealthComponent.body.AddTimedBuffAuthority(SS2Content.Buffs.bdEngiFocused.buffIndex, 5);
            //        }
            //
            //        //int count = victimHealthComponent.body.GetBuffCount(SS2Content.Buffs.bdEngiFocused);
            //        Debug.Log("count " + count);
            //        if (count > 0)
            //        {
            //            damageInfo.damage *= 1 + (count * .1f);
            //        }
            //    }
            //}
        }
    }
}
