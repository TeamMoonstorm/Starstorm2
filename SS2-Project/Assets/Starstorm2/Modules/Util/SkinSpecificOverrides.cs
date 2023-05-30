using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using R2API;
using R2API.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Modules
{
    public static class SkinSpecificOverrides
    {
        public static SkillDef phaseRoundDef;
        public static EntityStateConfiguration phaseRoundESC;

        internal static Material matLunarGolem;
        [SystemInitializer]
        public static void Initialize()
        {
            //Generic Hooks (Currently all Commando - subject to change)
            On.EntityStates.GenericProjectileBaseState.FireProjectile += GPBS_FireProjectile;
            On.EntityStates.GenericBulletBaseState.FireBullet += GBBS_FireBullet;
            CharacterBody.onBodyStartGlobal += BodyStartGlobal;

            //MUL-T specific
            matLunarGolem = Object.Instantiate(Resources.Load<GameObject>("Prefabs/CharacterBodies/LunarGolemBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial); // to-do: update this into an addressable
            On.EntityStates.Toolbot.BaseNailgunState.FireBullet += BaseNailgunState_FireBullet;
            On.EntityStates.Toolbot.FireSpear.FireBullet += FireSpear_FireBullet;
            On.EntityStates.Toolbot.ToolbotDualWield.OnEnter += ToolbotDualWield_OnEnter;
            On.EntityStates.Toolbot.ToolbotDash.OnEnter += ToolbotDash_OnEnter;
            //On.EntityStates.Toolbot.ToolbotDash.OnExit += ToolbotDash_OnExit;
        }

        public static void GPBS_FireProjectile(On.EntityStates.GenericProjectileBaseState.orig_FireProjectile orig, EntityStates.GenericProjectileBaseState self)
        {
            //Debug.Log("firing projectile");

            //There might be a better way to do this to ensure compatiability with mods that edit Phase Round, such as RiskyMod. Whatever that way is, I do not know of it.
            if (self.characterBody.baseNameToken == "COMMANDO_BODY_NAME") //name tokens never change :D
            {
                if (self.projectilePrefab == ProjectileCatalog.GetProjectilePrefab(ProjectileCatalog.FindProjectileIndex("FMJRamping")) && self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "SS2_SKIN_COMMANDO_VESTIGE")
                {
                    //grab the projectile prefab & modify it to use red vfx
                    GameObject projectileInstance;
                    projectileInstance = self.projectilePrefab;
                    ProjectileController pc = projectileInstance.GetComponent<ProjectileController>();
                    ProjectileOverlapAttack poa = projectileInstance.GetComponent<ProjectileOverlapAttack>();

                    pc.ghostPrefab = SS2Assets.LoadAsset<GameObject>("FMJRampingGhostRed", SS2Bundle.NemCommando);
                    poa.impactEffect = SS2Assets.LoadAsset<GameObject>("OmniExplosionVFXFMJRed", SS2Bundle.NemCommando);
                    self.effectPrefab = SS2Assets.LoadAsset<GameObject>("MuzzleflashNemCommandoRed", SS2Bundle.NemCommando);
                }
            }

            orig(self);
        }

        private static void BodyStartGlobal(CharacterBody body)
        {
            if (!NetworkServer.active)
                return;
            
            if (body.baseNameToken == "TOOLBOT_BODY_NAME")
            {
                //Debug.Log("is toolbot");
                if (body.modelLocator.modelBaseTransform.GetComponentInChildren<ModelSkinController>().skins[body.skinIndex].nameToken == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
                {
                    //Debug.Log("is lunar");
                    LoopSoundWhileCharacterMoving lswcm = body.GetComponent<LoopSoundWhileCharacterMoving>();
                    //lswcm.enabled = false;
                    lswcm.startSoundName = "Play_lunar_golem_idle_loop";
                    lswcm.stopSoundName = "Stop_lunar_golem_idle_loop";
                    lswcm.minSpeed = 0;
                    //it's just like an idle sound except i couldn't get it to actually work as an idle sound for some reason..

                    //Debug.Log("modified lunar idle sounds! :)");
                }
            }
        }

        public static void GBBS_FireBullet(On.EntityStates.GenericBulletBaseState.orig_FireBullet orig, EntityStates.GenericBulletBaseState self, Ray aimRay)
        {
            //Commando
            if (self.characterBody.baseNameToken == "COMMANDO_BODY_NAME")
            {
                //if using the skin, update vfx to use nemcommando variants
                if (self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "SS2_SKIN_COMMANDO_VESTIGE")
                {
                    if (self.tracerEffectPrefab == Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoShotgun.prefab").WaitForCompletion())
                    {
                        self.tracerEffectPrefab = SS2Assets.LoadAsset<GameObject>("TracerNemCommandoShotgunRed", SS2Bundle.NemCommando);
                    }
                    if (self.muzzleFlashPrefab == Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").WaitForCompletion())
                    {
                        self.muzzleFlashPrefab = SS2Assets.LoadAsset<GameObject>("MuzzleflashNemCommandoRed", SS2Bundle.NemCommando);
                    }
                    if (self.hitEffectPrefab == Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommandoShotgun.prefab").WaitForCompletion())
                    {
                        self.hitEffectPrefab = SS2Assets.LoadAsset<GameObject>("HitsparkNemCommandoRed", SS2Bundle.NemCommando);
                    }
                }
            }
            orig(self, aimRay);  
        }

        /*public static void ToolbotDash_OnExit(On.EntityStates.Toolbot.ToolbotDash.orig_OnExit orig, EntityStates.Toolbot.ToolbotDash self)
        {
            string skinNameToken = self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken;

            if (skinNameToken == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                SkateSparks ss = self.characterBody.modelLocator.modelTransform.GetComponent<SkateSparks>();
                GameObject toolbot = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Toolbot/ToolbotBody.prefab").WaitForCompletion();
                Transform toolbotModelTransform = null;
                if (toolbot != null)
                    toolbotModelTransform = toolbot.transform.GetComponent<ModelLocator>().modelBaseTransform;
                SkateSparks defaultSS = null;
                if (toolbotModelTransform != null)
                    defaultSS = toolbotModelTransform.GetComponent<SkateSparks>();
                if (defaultSS != null)
                {
                    ss.leftParticleSystem = defaultSS.leftParticleSystem;
                    ss.rightParticleSystem = defaultSS.rightParticleSystem;
                }
            }

            orig(self);
        }*/

        public static void ToolbotDash_OnEnter(On.EntityStates.Toolbot.ToolbotDash.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDash self)
        {
            bool isLunar = false;
            string oldEnterSound = EntityStates.Toolbot.ToolbotDash.startSoundString;
            string oldExitSound = EntityStates.Toolbot.ToolbotDash.endSoundString;
            string skinNameToken = self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken;

            if (skinNameToken == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                isLunar = true;
                EntityStates.Toolbot.ToolbotDash.startSoundString = EntityStates.LunarGolem.ChargeTwinShot.chargeSoundString;
                EntityStates.Toolbot.ToolbotDash.endSoundString = "Play_lunar_golem_death";

                /*SkateSparks ss = self.characterBody.modelLocator.modelTransform.GetComponent<SkateSparks>();
                GameObject lunarChimaera = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarGolem/LunarGolemBody.prefab").WaitForCompletion();
                Transform chimaeraModelTransform = null;
                if (lunarChimaera != null)
                    chimaeraModelTransform = lunarChimaera.transform.GetComponent<ModelLocator>().modelBaseTransform;
                SprintEffectController sec = null;
                if (chimaeraModelTransform != null)
                    sec = chimaeraModelTransform.GetComponent<SprintEffectController>();
                ParticleSystem lcDebris = null;
                if (sec != null)
                    lcDebris = sec.loopSystems[1];

                ss.leftParticleSystem = lcDebris;
                ss.rightParticleSystem = lcDebris;*/
            }

            orig(self);

            if (isLunar)
            {
                EntityStates.Toolbot.ToolbotDash.startSoundString = oldEnterSound;
                EntityStates.Toolbot.ToolbotDash.endSoundString = oldExitSound;
            }    
        }

        public static void ToolbotDualWield_OnEnter(On.EntityStates.Toolbot.ToolbotDualWield.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDualWield self)
        {
            bool isLunar = false;
            string oldSound = EntityStates.Toolbot.ToolbotDualWieldStart.enterSfx;
            string skinNameToken = self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken;

            if (skinNameToken == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                //if using the lunar skin, set to lunar & update sound
                isLunar = true;
                EntityStates.Toolbot.ToolbotDualWieldStart.enterSfx = EntityStates.LunarGolem.ChargeTwinShot.chargeSoundString;
            }

            //call self
            orig(self);

            if (isLunar)
            {
                //change mats of dual wield guns to lunar
                self.coverLeftInstance.GetComponentInChildren<SkinnedMeshRenderer>().material = matLunarGolem;
                self.coverRightInstance.GetComponentInChildren<SkinnedMeshRenderer>().material = matLunarGolem;

                //change sound back after skill is played to ensure non-lunar toolbots still function correctly
                EntityStates.Toolbot.ToolbotDualWieldStart.enterSfx = oldSound;
            }
        }

        public static void FireSpear_FireBullet(On.EntityStates.Toolbot.FireSpear.orig_FireBullet orig, EntityStates.Toolbot.FireSpear self, Ray aimRay)
        {
            bool isLunar = false;
            GameObject oldTracer = self.tracerEffectPrefab;

            //check skin; update tracer if in use
            if (self.GetModelTransform().GetComponentInChildren<ModelSkinController>().skins[self.characterBody.skinIndex].nameToken == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                isLunar = true;
                self.tracerEffectPrefab = Resources.Load<GameObject>("Prefabs/Effects/Tracers/TracerHuntressSnipe"); // this too
            }

            //call self
            orig(self, aimRay);

            //change tracer back
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