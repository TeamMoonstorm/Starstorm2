using Assets.Starstorm2.ContentClasses;
using SS2;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using RoR2;
using MSU;
using RoR2.Skills;
using RoR2.ContentManagement;

namespace Assets.Starstorm2.Modules.Characters.Variants
{
    public class Acrid : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<AssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<AssetCollection>("acAcrid", SS2Bundle.Indev);


        public override void Initialize()
        {
            SkillDef sdCorrodingSpit = survivorAssetCollection.FindAsset<SkillDef>("sdCorrodingSpit");

            GameObject acridBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = acridBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;

            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdCorrodingSpit,
                viewableNode = new ViewablesCatalog.Node(sdCorrodingSpit.skillNameToken, false, null)
            };
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
