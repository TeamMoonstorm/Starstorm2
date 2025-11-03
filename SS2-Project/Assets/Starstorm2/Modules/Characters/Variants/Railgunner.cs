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
            GameObject railgunnerBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = railgunnerBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily utilitySkillFamility = skillLocator.utility.skillFamily;


            SkillDef railGunnerRoll = assetCollection.FindAsset<SkillDef>("sdRailRoll");

            AddSkill(utilitySkillFamility, railGunnerRoll);
        }

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
