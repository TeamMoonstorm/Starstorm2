using Assets.Starstorm2.ContentClasses;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using SS2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace SS2.Survivors
{
    public sealed class Commando : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<AssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<AssetCollection>("acCommando", SS2Bundle.Indev);
        

        public override void Initialize()
        {
            SkillDef sdDeadeye = survivorAssetCollection.FindAsset<SkillDef>("sdDeadeye");
            SkillDef sdDirtbomb = survivorAssetCollection.FindAsset<SkillDef>("sdDirtbomb");

            GameObject commandoBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = commandoBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamily = skillLocator.primary.skillFamily;
            SkillFamily specialSkillFamily = skillLocator.special.skillFamily;

            // If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the existing Skill Family.
            Array.Resize(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdDeadeye,
                viewableNode = new ViewablesCatalog.Node(sdDeadeye.skillNameToken, false, null)
            };

            Array.Resize(ref specialSkillFamily.variants, specialSkillFamily.variants.Length + 1);
            specialSkillFamily.variants[specialSkillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdDirtbomb,
                viewableNode = new ViewablesCatalog.Node(sdDirtbomb.skillNameToken, false, null)
            };
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
