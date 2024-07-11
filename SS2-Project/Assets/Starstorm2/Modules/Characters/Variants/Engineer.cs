using Assets.Starstorm2.ContentClasses;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using SS2;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace SS2.Survivors
{
    public class Engineer : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<AssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<AssetCollection>("acEngineer", SS2Bundle.Indev);


        public override void Initialize()
        {
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
        }


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
