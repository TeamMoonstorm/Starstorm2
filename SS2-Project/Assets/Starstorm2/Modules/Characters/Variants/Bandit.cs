using Assets.Starstorm2.ContentClasses;
using SS2;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using MSU;
using RoR2.Skills;
using RoR2.ContentManagement;
using R2API;

namespace SS2.Survivors
{
    public class Bandit : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<AssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<AssetCollection>("acBandit", SS2Bundle.Indev);

        public static DamageAPI.ModdedDamageType TranqDamageType { get; set; }

        public override void Initialize()
        {
            TranqDamageType = DamageAPI.ReserveDamageType();

            SkillDef sdTranquilizerGun = survivorAssetCollection.FindAsset<SkillDef>("sdTranquilizerGun");

            GameObject acridBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Bandit2/Bandit2Body.prefab").WaitForCompletion();

            SkillLocator skillLocator = acridBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdTranquilizerGun,
                viewableNode = new ViewablesCatalog.Node(sdTranquilizerGun.skillNameToken, false, null)
            };
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
