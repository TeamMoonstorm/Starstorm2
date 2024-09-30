using SS2;
using RoR2;
using RoR2.ContentManagement;
using MSU;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2.Skills;

namespace SS2.Survivors
{
    public class Railgunner : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acRailgunner", SS2Bundle.Indev);


        public override void Initialize()
        {
            //ModifyPrefab();
        }

        private void ModifyPrefab()
        {
            SkillDef sdPierceRifle = assetCollection.FindAsset<SkillDef>("sdPierceRifle");
            SkillDef sdAssaultRifle = assetCollection.FindAsset<SkillDef>("sdAssaultRifle");

            GameObject railgunnerBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = railgunnerBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            // If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the existing Skill Family.
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdPierceRifle,
                viewableNode = new ViewablesCatalog.Node(sdPierceRifle.skillNameToken, false, null)
            };

            // TODO: Clean this up
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdAssaultRifle,
                viewableNode = new ViewablesCatalog.Node(sdPierceRifle.skillNameToken, false, null)
            };
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }
    }
}
