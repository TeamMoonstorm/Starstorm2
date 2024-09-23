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
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acEngineer", SS2Bundle.Indev);

        public static ModdedDamageType EngiFocusDamage { get; private set; }
        public static ModdedDamageType EngiFocusDamageProc { get; private set; }
        public static BuffDef _buffDefEngiFocused;

        public static GameObject displacementGroundHitbox;
        public static GameObject engiPrefabExplosion;
        public override void Initialize()
        {
            //_buffDefEngiFocused = survivorAssetCollection.FindAsset<BuffDef>("bdEngiFocused");

            SkillDef sdLaserFocus = assetCollection.FindAsset<SkillDef>("sdLaserFocus");
            SkillDef sdRapidDisplacement = assetCollection.FindAsset<SkillDef>("sdRapidDisplacement");
            SkillDef sdQuantumTranslocator = assetCollection.FindAsset<SkillDef>("sdQuantumTranslocator");

            GameObject engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();


            var modelTransform = engiBodyPrefab.GetComponent<ModelLocator>().modelTransform;
            GameObject groundbox = assetCollection.FindAsset<GameObject>("HitboxGround");
            GameObject hopbox = assetCollection.FindAsset<GameObject>("HitboxHop");

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

            engiPrefabExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Engi/EngiConcussionExplosion.prefab").WaitForCompletion(); //= PrefabAPI.InstantiateClone(Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Engi/EngiConcussionExplosion.prefab").WaitForCompletion(), "engiDashExplosion");
                                                                                                                                                //GameObject.Destroy(engiPrefabExplosion.GetComponent<EffectComponent>());

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
            SkillFamily skillFamilySecondary = skillLocator.secondary.skillFamily;

            AddSkill(skillFamilyPrimary, sdLaserFocus);
            //AddSkill(skillFamilyUtility, sdQuantumTranslocator);
            AddSkill(skillFamilyUtility, sdRapidDisplacement);

            EngiFocusDamage = DamageAPI.ReserveDamageType();
            EngiFocusDamageProc = DamageAPI.ReserveDamageType();

            On.RoR2.HealthComponent.TakeDamage += EngiFocusDamageHook;
            On.RoR2.EffectComponent.Start += StopDoingThat;
        }

        private void StopDoingThat(On.RoR2.EffectComponent.orig_Start orig, EffectComponent self)
        {

            orig(self);
            if (self && self.effectData != null && self.effectData.genericFloat == -23)
            {
                self.transform.localPosition = self.effectData.origin;
                self.transform.localScale = new Vector3(self.effectData.scale, self.effectData.scale, self.effectData.scale);

            }
        }

        private void EngiFocusDamageHook(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            var dmg = damageInfo.HasModdedDamageType(EngiFocusDamage);
            var proc = damageInfo.HasModdedDamageType(EngiFocusDamageProc);
            if (dmg || proc)
            {
                int count = self.body.GetBuffCount(SS2Content.Buffs.bdEngiFocused);
                if (proc && count < 5)
                {
                    SS2Util.RefreshAllBuffStacks(self.body, SS2Content.Buffs.bdEngiFocused, 5);
                    self.body.AddTimedBuffAuthority(SS2Content.Buffs.bdEngiFocused.buffIndex, 5);

                }
                else if (proc)
                {
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

        public override void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(assetCollection);
        }

        //what even is this -N
        public sealed class EngiFocusedBuffBehavior : BaseBuffBehaviour
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdEngiFocused;
        }
    }
}
