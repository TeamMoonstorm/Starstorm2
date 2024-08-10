using Assets.Starstorm2.ContentClasses;
using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;

namespace SS2.Survivors
{
    public class VoidFiend : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<AssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<AssetCollection>("acVoidFiend", SS2Bundle.Indev);

        SkillDef sdUncorruptedSwipe;
        SkillDef sdCorruptedSwipe;
        public override void Initialize()
        {
            sdUncorruptedSwipe = survivorAssetCollection.FindAsset<SkillDef>("sdUncorruptedSwipe");
            sdCorruptedSwipe = survivorAssetCollection.FindAsset<SkillDef>("sdCorruptedSwipe");

            GameObject voidSurvivorBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/VoidSurvivorBody.prefab").WaitForCompletion();

            SkillLocator skillLocator = voidSurvivorBodyPrefab.GetComponent<SkillLocator>();
            Debug.Log("skillLocator: " + skillLocator + " | sduncorrupt: " + sdUncorruptedSwipe + " |sdcorrupt " + sdUncorruptedSwipe);
            SkillFamily skillFamilySpecial = skillLocator.special.skillFamily;
            Debug.Log("skillFamilySpecial: " + skillFamilySpecial + " | " + skillFamilySpecial.variants);
            // If this is an alternate skill, use this code.
            // Here, we add our skill as a variant to the existing Skill Family.
            Array.Resize(ref skillFamilySpecial.variants, skillFamilySpecial.variants.Length + 1);
            Debug.Log("woooo");
            skillFamilySpecial.variants[skillFamilySpecial.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdUncorruptedSwipe,
                viewableNode = new ViewablesCatalog.Node(sdUncorruptedSwipe.skillNameToken, false, null)
            };

            On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.OnEnter += FixAlts;

        }

        private void FixAlts(On.EntityStates.VoidSurvivor.CorruptMode.CorruptMode.orig_OnEnter orig, EntityStates.VoidSurvivor.CorruptMode.CorruptMode self)
        {
            Debug.Log("AHHH!!!: " + self.skillLocator.special.skillDef + " | " + sdUncorruptedSwipe);
            if (self.skillLocator.special.skillDef == sdUncorruptedSwipe)
            {
                Debug.Log("AHHH!!!");
                self.specialOverrideSkillDef = sdCorruptedSwipe;
                Debug.Log("Overriding with new speical skill");
            }
            orig(self);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
