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

        public static GameObject displacementGroundHitbox;
        public static GameObject engiPrefabExplosion;
        public override void Initialize()
        {
            //_buffDefEngiFocused = survivorAssetCollection.FindAsset<BuffDef>("bdEngiFocused");

            SkillDef sdLaserFocus = survivorAssetCollection.FindAsset<SkillDef>("sdLaserFocus");
            SkillDef sdRapidDisplacement = survivorAssetCollection.FindAsset<SkillDef>("sdRapidDisplacement");

            GameObject engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();


            var modelTransform  = engiBodyPrefab.GetComponent<ModelLocator>().modelTransform;
            GameObject groundbox = survivorAssetCollection.FindAsset<GameObject>("HitboxGround");
            GameObject hopbox = survivorAssetCollection.FindAsset<GameObject>("HitboxHop");

            groundbox.transform.parent = modelTransform;
            hopbox.transform.parent = modelTransform;
            groundbox.transform.localPosition = new Vector3(0, 1.5f, 0);
            hopbox.transform.localPosition = new Vector3(0, 0.65f, -1);
            groundbox.transform.localRotation = Quaternion.Euler(Vector3.zero);
            hopbox.transform.localRotation = Quaternion.Euler(Vector3.zero);
            groundbox.transform.localScale = new Vector3(3.25f, 3.25f, 3.25f);
            hopbox.transform.localScale = new Vector3(4.25f, 4.25f, 4.25f);

            var hbg1 = modelTransform.gameObject.AddComponent<HitBoxGroup>();
            hbg1.groupName = "HitboxGround";
            hbg1.hitBoxes = new HitBox[1];
            hbg1.hitBoxes[0] = groundbox.GetComponent<HitBox>();

            var hbg2 = modelTransform.gameObject.AddComponent<HitBoxGroup>();
            hbg2.groupName = "HitboxHop";
            hbg2.hitBoxes = new HitBox[1];
            hbg2.hitBoxes[0] = hopbox.GetComponent<HitBox>();

            engiPrefabExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Engi/EngiConcussionExplosion.prefab").WaitForCompletion();

            //engiExplosionLeft = survivorAssetCollection.FindAsset<GameObject>("EngiConcussionExplosion").InstantiateClone("LeftExplosion");
            //engiExplosionRight = survivorAssetCollection.FindAsset<GameObject>("EngiConcussionExplosion").InstantiateClone("RightExplosion");
            //engiExplosionLeft.transform.parent = modelTransform;
            //engiExplosionRight.transform.parent = modelTransform;
            //
            //engiExplosionLeft.transform.localPosition = new Vector3(-.325f, 2.1f, -.7f);
            //engiExplosionRight.transform.localPosition = new Vector3(.325f, 2.1f, -.7f);
            //engiExplosionLeft.transform.localRotation = Quaternion.Euler(Vector3.zero);
            //engiExplosionRight.transform.localRotation = Quaternion.Euler(Vector3.zero);
            //engiExplosionLeft.SetActive(false);
            //engiExplosionRight.SetActive(false);

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

            Array.Resize(ref skillFamilyUtility.variants, skillFamilyUtility.variants.Length + 1);
            skillFamilyUtility.variants[skillFamilyUtility.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = sdRapidDisplacement,
                viewableNode = new ViewablesCatalog.Node(sdRapidDisplacement.skillNameToken, false, null)
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
