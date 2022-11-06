using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using R2API;
using R2API.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;

namespace Moonstorm.Starstorm2.Modules
{
    public static class UglyMULT
    {
        internal static Material matLunarGolem;
        [SystemInitializer]
        public static void Initialize()
        {
            matLunarGolem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/LunarGolemBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial); // to-do: update this into an addressable

            On.EntityStates.Toolbot.BaseNailgunState.FireBullet += BaseNailgunState_FireBullet;
            On.EntityStates.Toolbot.FireSpear.FireBullet += FireSpear_FireBullet;
            On.EntityStates.Toolbot.ToolbotDualWield.OnEnter += ToolbotDualWield_OnEnter;
        }

        public static void ToolbotDualWield_OnEnter(On.EntityStates.Toolbot.ToolbotDualWield.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDualWield self)
        {
            orig(self);

            if (self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                self.coverLeftInstance.GetComponentInChildren<SkinnedMeshRenderer>().material = matLunarGolem;
                self.coverRightInstance.GetComponentInChildren<SkinnedMeshRenderer>().material = matLunarGolem;
            }
        }

        public static void FireSpear_FireBullet(On.EntityStates.Toolbot.FireSpear.orig_FireBullet orig, EntityStates.Toolbot.FireSpear self, Ray aimRay)
        {
            bool isLunar = false;
            GameObject oldTracer = self.tracerEffectPrefab;

            if (self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                isLunar = true;
                self.tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerHuntressSnipe"); // this too
            }

            orig(self, aimRay);

            if (isLunar)
            {
                self.tracerEffectPrefab = oldTracer;
            }
        }

        private static void BaseNailgunState_FireBullet(On.EntityStates.Toolbot.BaseNailgunState.orig_FireBullet orig, EntityStates.Toolbot.BaseNailgunState self, Ray aimRay, int bulletCount, float spreadPitchScale, float spreadYawScale)
        {
            bool isLunar = false;
            GameObject oldTracer = EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab;
            string oldSound = EntityStates.Toolbot.BaseNailgunState.fireSoundString;

            if (self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                isLunar = true;
                EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerLunarWispMinigun"); // and also this
                EntityStates.Toolbot.BaseNailgunState.fireSoundString = EntityStates.LunarWisp.FireLunarGuns.fireSound;

            }

            orig(self, aimRay, bulletCount, spreadPitchScale, spreadYawScale);

            if (isLunar)
            {
                EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab = oldTracer;
                EntityStates.Toolbot.BaseNailgunState.fireSoundString = oldSound;
            }
        }
    }
}