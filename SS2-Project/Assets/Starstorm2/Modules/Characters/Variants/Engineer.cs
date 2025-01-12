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
using Path = System.IO.Path;
using R2API.Models;
using System.Collections.Generic;

namespace SS2.Survivors
{
    public class Engineer : SS2VanillaSurvivor
    {
        public override SS2AssetRequest<VanillaSurvivorAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<VanillaSurvivorAssetCollection>("acEngineer", SS2Bundle.Indev);
        private static string RuntimePath = Path.Combine(Addressables.RuntimePath, "StandaloneWindows64");
        private static string AnimationPath = Path.Combine(Path.GetDirectoryName(SS2Main.Instance.Info.Location), "assetbundles", "ss2dev");

        public static ModdedDamageType EngiFocusDamage { get; private set; }
        public static ModdedDamageType EngiFocusDamageProc { get; private set; }
        public static BuffDef _buffDefEngiFocused;

        public static GameObject displacementGroundHitbox;
        public static GameObject engiPrefabExplosion;
        public override void Initialize()
        {
            GameObject engiBodyPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion();
            string engi = Path.Combine(RuntimePath, "ror2-base-engi_text_assets_all.bundle");
            var runtimeEngi = Addressables.LoadAssetAsync<RuntimeAnimatorController>("RoR2/Base/Engi/animEngi.controller").WaitForCompletion();
            AnimatorModifications engiMods = new AnimatorModifications(SS2Main.Instance.Info.Metadata);

            List<State> states = SetupEngiLaserStates();
            engiMods.NewStates.Add("Gesture, Additive", states);
            AnimationsAPI.AddModifications(engi, runtimeEngi, engiMods);
            var animEngi = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiBody.prefab").WaitForCompletion().GetComponentInChildren<Animator>();
            AnimationsAPI.AddAnimatorController(animEngi, runtimeEngi);

            SkillDef sdLaserFocus = assetCollection.FindAsset<SkillDef>("sdLaserFocus");
            SkillDef sdRapidDisplacement = assetCollection.FindAsset<SkillDef>("sdRapidDisplacement");
            SkillDef sdQuantumTranslocator = assetCollection.FindAsset<SkillDef>("sdQuantumTranslocator");

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

            engiPrefabExplosion = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Engi/EngiConcussionExplosion.prefab").WaitForCompletion();

            SkillLocator skillLocator = engiBodyPrefab.GetComponent<SkillLocator>();
            SkillFamily skillFamilyPrimary = skillLocator.primary.skillFamily;
            SkillFamily skillFamilyUtility = skillLocator.utility.skillFamily;
            SkillFamily skillFamilySecondary = skillLocator.secondary.skillFamily;

            AddSkill(skillFamilyPrimary, sdLaserFocus);
            AddSkill(skillFamilyUtility, sdQuantumTranslocator);
            AddSkill(skillFamilyUtility, sdRapidDisplacement);

            EngiFocusDamage = DamageAPI.ReserveDamageType();
            EngiFocusDamageProc = DamageAPI.ReserveDamageType();

            On.RoR2.HealthComponent.TakeDamage += EngiFocusDamageHook;
            On.RoR2.EffectComponent.Start += StopDoingThat;
        }

        private List<State> SetupEngiLaserStates()
        {
            Parameter laserFocusPlaybackRate = new Parameter
            {
                Type = ParameterType.Float,
                Name = "LaserFocus.playbackRate",
                Value = 0f
            };

            State chargeState = new State
            {
                ClipBundlePath = AnimationPath,
                Speed = 1,
                SpeedParam = laserFocusPlaybackRate.Name,
                WriteDefaultValues = true,
                Name = "SS2-ChargeLaserFocus",
                Clip = assetCollection.FindAsset<AnimationClip>("EngiArmature_laserStart")
            };

            State idleState = new State
            {
                ClipBundlePath = AnimationPath,
                Speed = 1,
                SpeedParam = laserFocusPlaybackRate.Name,
                WriteDefaultValues = true,
                Name = "SS2-IdleLaserFocus",
                Clip = assetCollection.FindAsset<AnimationClip>("EngiArmature_laserIdle")
            };

            State bapState = new State
            {
                ClipBundlePath = AnimationPath,
                Speed = 1,
                SpeedParam = laserFocusPlaybackRate.Name,
                WriteDefaultValues = true,
                Name = "SS2-BapLaserFocus",
                Clip = assetCollection.FindAsset<AnimationClip>("EngiArmature_laserBap")
            };

            State exitState = new State
            {
                ClipBundlePath = AnimationPath,
                Speed = 1,
                SpeedParam = laserFocusPlaybackRate.Name,
                WriteDefaultValues = true,
                Name = "SS2-ExitLaserFocus",
                Clip = assetCollection.FindAsset<AnimationClip>("EngiArmature_laserEnd")
            };
            var exitTransition = new Transition
            {
                DestinationStateName = "Empty 0",
                TransitionDuration = .2f,
                ExitTime = 0.9f,
                HasExitTime = true
            };
            exitState.Transitions.Add(exitTransition);


            return new List<State> { chargeState, exitState, idleState, bapState };
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
            bool dmg = damageInfo.HasModdedDamageType(EngiFocusDamage);
            bool proc = damageInfo.HasModdedDamageType(EngiFocusDamageProc);

            int count = self.body.GetBuffCount(SS2Content.Buffs.bdEngiFocused);
            if (dmg || proc)
            {
                if (count > 0)
                {
                    damageInfo.damage *= 1 + (count * .15f);
                }
            }

            if (proc)
            {
                SS2Util.RefreshAllBuffStacks(self.body, SS2Content.Buffs.bdEngiFocused, 5);
                if (count < 5)
                {
                    self.body.AddTimedBuffAuthority(SS2Content.Buffs.bdEngiFocused.buffIndex, 5);
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
