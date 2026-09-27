using System;
using System.Collections;
using System.Linq;
using HG;
using MSU;
using UnityEngine;
using RoR2;
using RoR2.Skills;
using UnityEngine.AddressableAssets;
using RoR2.Projectile;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using AffixBeadAttachment = On.RoR2.AffixBeadAttachment;
using Object = UnityEngine.Object;

namespace SS2.Modules
{
    public static class SkinSpecificOverrides
    {
        //base for overrides
        private static GameObject FMJRampingPrefab;
        private static GameObject tracerCommandoShotgun; 
        private static GameObject muzzleflashFMJ; 
        private static GameObject hitsparkCommandoShotgun; 
        
        //nemmando. ,.
        private static GameObject FMJRampingGhostRed;
        private static GameObject omniExplosionVFXFMJRed;
        private static GameObject tracerNemCommandoShotgunRed; 
        private static GameObject muzzleflashNemCommandoRed; 
        private static GameObject hitsparkNemCommandoRed; 
        
        //specialistt. ,.
        private static GameObject FMJRampingGhostSpecialist;
        private static GameObject omniExplosionVFXFMJSpecialist;
        private static GameObject tracerCommandoShotgunSpecialist; 
        private static GameObject muzzleflashCommandoSpecialist; 
        private static GameObject hitsparkCommandoSpecialist; 

        //mult .,,.
        private static GameObject toolbotLunarSpear;
        private static GameObject lunarWispMinigunTracer;
        private static Material matLunarGolem;
        
        //chirr,.,.
        private static int isopodLocalSkinIndex = -1;
        
        [SystemInitializer]
        public static void Initialize()
        {
            //Generic Hooks
            On.EntityStates.GenericProjectileBaseState.FireProjectile += GPBS_FireProjectile;
            On.EntityStates.GenericBulletBaseState.FireBullet += GBBS_FireBullet;
            CharacterBody.onBodyStartGlobal += BodyStartGlobal;
            On.RoR2.ModelSkinController.Awake += ModelSkinControllerOnAwake;

            //MUL-T specific
            On.EntityStates.Toolbot.BaseNailgunState.FireBullet += BaseNailgunState_FireBullet;
            On.EntityStates.Toolbot.FireSpear.FireBullet += FireSpear_FireBullet;
            On.EntityStates.Toolbot.ToolbotDualWield.OnEnter += ToolbotDualWield_OnEnter;
            On.EntityStates.Toolbot.ToolbotDash.OnEnter += ToolbotDash_OnEnter;
            //On.EntityStates.Toolbot.ToolbotDash.OnExit += ToolbotDash_OnExit;
        }

        private static void ModelSkinControllerOnAwake(On.RoR2.ModelSkinController.orig_Awake orig, ModelSkinController self)
        {
            orig(self);

            if (self.gameObject.name == "mdlChirr")
            {
                for (int i = 0; i < self.skins.Length; i++)
                {
                    if (self.skins[i].nameToken != "SS2_SKIN_CHIRR_ISOPOD") continue;
                    
                    isopodLocalSkinIndex = i;
                    break;
                }

                if (self.currentSkinIndex == isopodLocalSkinIndex)
                {
                    Transform isopodTransform = self.gameObject.transform.Find("ChirrIsopodWings");
                    if (isopodTransform)
                    {
                        isopodTransform.gameObject.SetActive(true);
                        if (isopodTransform.gameObject.TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer))
                        {
                            skinnedMeshRenderer.enabled = true;
                        }
                    }
                }
                
                self.onSkinApplied += i =>
                {
                    bool enableWings = isopodLocalSkinIndex == i;
                    Transform isopodTransform = self.gameObject.transform.Find("ChirrIsopodWings");
                    if (isopodTransform)
                    {
                        isopodTransform.gameObject.SetActive(enableWings);
                        if (isopodTransform.gameObject.TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer))
                        {
                            skinnedMeshRenderer.enabled = enableWings;
                        }
                    }
                };
            }
        }

        //projectile catalog for FMJRampingPrefab or others ./,..
        [SystemInitializer(typeof(ProjectileCatalog), typeof(EffectCatalog))]
        public static void LoadBasePrefabs()
        {
            //mult gm
            Addressables.LoadAssetAsync<Material>("RoR2/Base/LunarGolem/matLunarGolem.mat").Completed += handle =>
            {
                matLunarGolem = handle.Result;
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/TracerHuntressSnipe.prefab").Completed += handle =>
            {
                toolbotLunarSpear = handle.Result;
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/LunarWisp/TracerLunarWispMinigun.prefab").Completed += handle =>
            {
                lunarWispMinigunTracer = handle.Result;
            };
            
            //commando 
            FMJRampingPrefab = ProjectileCatalog.GetProjectilePrefab(ProjectileCatalog.FindProjectileIndex("FMJRamping"));
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/TracerCommandoShotgun.prefab").Completed += handle =>
            {
                tracerCommandoShotgun = handle.Result;
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/MuzzleflashFMJ.prefab").Completed += handle =>
            {
                muzzleflashFMJ = handle.Result;
            };
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/HitsparkCommandoShotgun.prefab").Completed += handle =>
            {
                hitsparkCommandoShotgun = handle.Result;
            };
            
            //nem commando skin .,,
            FMJRampingGhostRed = SS2Assets.LoadAsset<GameObject>("FMJRampingGhostRed", SS2Bundle.Vanilla);
            omniExplosionVFXFMJRed = SS2Assets.LoadAsset<GameObject>("OmniExplosionVFXFMJRed", SS2Bundle.Vanilla);
            tracerNemCommandoShotgunRed = SS2Assets.LoadAsset<GameObject>("TracerNemCommandoShotgunRed", SS2Bundle.NemCommando);
            muzzleflashNemCommandoRed = SS2Assets.LoadAsset<GameObject>("MuzzleflashNemCommandoRed", SS2Bundle.NemCommando);
            hitsparkNemCommandoRed = SS2Assets.LoadAsset<GameObject>("HitsparkNemCommandoRed", SS2Bundle.NemCommando);
            
            //specialist .,,.
            FMJRampingGhostSpecialist = SS2Assets.LoadAsset<GameObject>("FMJRampingGhostSpecialist", SS2Bundle.Vanilla);
            omniExplosionVFXFMJSpecialist = SS2Assets.LoadAsset<GameObject>("OmniExplosionVFXFMJSpecialist", SS2Bundle.Vanilla);
            tracerCommandoShotgunSpecialist = SS2Assets.LoadAsset<GameObject>("TracerCommandoShotgunSpecialist", SS2Bundle.Vanilla);
            muzzleflashCommandoSpecialist = SS2Assets.LoadAsset<GameObject>("MuzzleflashCommandoSpecialist", SS2Bundle.Vanilla);
            hitsparkCommandoSpecialist = SS2Assets.LoadAsset<GameObject>("HitsparkCommandoShotgunSpecialist", SS2Bundle.Vanilla);
            
            //chirr ,..,
            chirrIsopodWingPrefab = SS2Assets.LoadAsset<GameObject>("ChirrIsopodWings", SS2Bundle.Chirr);
            chirrIsopodWingMat = SS2Assets.LoadAsset<Material>("matChirrIsopodWing", SS2Bundle.Chirr);
        }
        
        private static string GetSkinName(CharacterBody body)
        {
            return body?.modelLocator?.modelTransform?.GetComponentInChildren<ModelSkinController>()?.skins[body.skinIndex].nameToken;
        }
        
        public static void GPBS_FireProjectile(On.EntityStates.GenericProjectileBaseState.orig_FireProjectile orig, EntityStates.GenericProjectileBaseState self)
        {
            //There might be a better way to do this to ensure compatiability with mods that edit Phase Round, such as RiskyMod. Whatever that way is, I do not know of it.
            if (self.characterBody.baseNameToken == "COMMANDO_BODY_NAME") //name tokens never change :D
            {
                if (self.projectilePrefab == FMJRampingPrefab)
                {
                    string skinName = GetSkinName(self.characterBody);
                    if (skinName == "SS2_SKIN_COMMANDO_VESTIGE")
                    {
                        GameObject projectileInstance = self.projectilePrefab;
                        ProjectileController pc = projectileInstance.GetComponent<ProjectileController>();
                        ProjectileOverlapAttack poa = projectileInstance.GetComponent<ProjectileOverlapAttack>();
                        
                        pc.ghostPrefab = FMJRampingGhostRed;
                        poa.impactEffect = omniExplosionVFXFMJRed;
                        self.effectPrefab = muzzleflashNemCommandoRed;
                    }
                    else if (skinName == "SS2_SKIN_COMMANDO_SPECIALIST")
                    {
                        GameObject projectileInstance = self.projectilePrefab;
                        ProjectileController pc = projectileInstance.GetComponent<ProjectileController>();
                        ProjectileOverlapAttack poa = projectileInstance.GetComponent<ProjectileOverlapAttack>();
                        
                        pc.ghostPrefab = FMJRampingGhostSpecialist;
                        poa.impactEffect = omniExplosionVFXFMJSpecialist;
                        self.effectPrefab = muzzleflashCommandoSpecialist;
                    }
                }
            }

            orig(self);
        }

        private static void BodyStartGlobal(CharacterBody body)
        {
            if (!NetworkServer.active) return;

            if (body.baseNameToken == "TOOLBOT_BODY_NAME")
            {
                if (GetSkinName(body) == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
                {
                    LoopSoundWhileCharacterMoving lswcm = body.GetComponent<LoopSoundWhileCharacterMoving>();

                    if (lswcm != null)
                    {
                        lswcm.startSoundName = "Play_lunar_golem_idle_loop";
                        lswcm.stopSoundName = "Stop_lunar_golem_idle_loop";
                        lswcm.minSpeed = 0;
                        //it's just like an idle sound except i couldn't get it to actually work as an idle sound for some reason..
                    }
                }
            }
        }
        
        public static void GBBS_FireBullet(On.EntityStates.GenericBulletBaseState.orig_FireBullet orig, EntityStates.GenericBulletBaseState self, Ray aimRay)
        {
            //Commando
            if (self.characterBody.baseNameToken == "COMMANDO_BODY_NAME")
            {
                //if using the skin, update vfx to use nemcommando variants
                string skinName = GetSkinName(self.characterBody);
                if (skinName == "SS2_SKIN_COMMANDO_VESTIGE")
                {
                    if (self.tracerEffectPrefab == tracerCommandoShotgun)
                        self.tracerEffectPrefab = tracerNemCommandoShotgunRed;
                    
                    if (self.muzzleFlashPrefab == muzzleflashFMJ)
                        self.muzzleFlashPrefab = muzzleflashNemCommandoRed;
                    
                    if (self.hitEffectPrefab == hitsparkCommandoShotgun)
                        self.hitEffectPrefab = hitsparkNemCommandoRed;
                }
                else if (skinName == "SS2_SKIN_COMMANDO_SPECIALIST")
                {
                    if (self.tracerEffectPrefab == tracerCommandoShotgun)
                        self.tracerEffectPrefab = tracerCommandoShotgunSpecialist;
                    
                    if (self.muzzleFlashPrefab == muzzleflashFMJ)
                        self.muzzleFlashPrefab = muzzleflashCommandoSpecialist;
                    
                    if (self.hitEffectPrefab == hitsparkCommandoShotgun)
                        self.hitEffectPrefab = hitsparkCommandoSpecialist;
                }
            }
            
            orig(self, aimRay);  
        }

        //unsure what this one does and the code looks a little scary and overcomplicated to 
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
            string oldEnterSound = EntityStates.Toolbot.ToolbotDash.startSoundString;
            string oldExitSound = EntityStates.Toolbot.ToolbotDash.endSoundString;

            if (GetSkinName(self.characterBody) == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
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

            if (EntityStates.Toolbot.ToolbotDash.startSoundString != oldEnterSound)
            {
                EntityStates.Toolbot.ToolbotDash.startSoundString = oldEnterSound;
                EntityStates.Toolbot.ToolbotDash.endSoundString = oldExitSound;
            }    
        }

        private static void ToolbotDualWield_OnEnter(On.EntityStates.Toolbot.ToolbotDualWield.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDualWield self)
        {
            string oldSound = EntityStates.Toolbot.ToolbotDualWieldStart.enterSfx;

            if (GetSkinName(self.characterBody) == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                //if using the lunar skin, set to lunar & update sound
                EntityStates.Toolbot.ToolbotDualWieldStart.enterSfx = EntityStates.LunarGolem.ChargeTwinShot.chargeSoundString;
            }

            //call self
            orig(self);

            if (EntityStates.Toolbot.ToolbotDualWieldStart.enterSfx != oldSound)
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
            //check skin; update tracer if in use
            if (GetSkinName(self.characterBody) == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                self.tracerEffectPrefab = toolbotLunarSpear;
            }

            //call self
            orig(self, aimRay);
        }

        private static void BaseNailgunState_FireBullet(On.EntityStates.Toolbot.BaseNailgunState.orig_FireBullet orig, EntityStates.Toolbot.BaseNailgunState self, Ray aimRay, int bulletCount, float spreadPitchScale, float spreadYawScale)
        {
            GameObject oldTracer = EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab;
            string oldSound = EntityStates.Toolbot.BaseNailgunState.fireSoundString;

            if (GetSkinName(self.characterBody) == "SS2_SKIN_TOOLBOT_GRANDMASTERY")
            {
                EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab = lunarWispMinigunTracer; // and also this
                EntityStates.Toolbot.BaseNailgunState.fireSoundString = EntityStates.LunarWisp.FireLunarGuns.fireSound;
            }

            orig(self, aimRay, bulletCount, spreadPitchScale, spreadYawScale);

            if (EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab != oldTracer)
            {
                EntityStates.Toolbot.BaseNailgunState.tracerEffectPrefab = oldTracer;
                EntityStates.Toolbot.BaseNailgunState.fireSoundString = oldSound;
            }
        }

        private static void ModifiyLighting(On.RoR2.SkinDef.orig_Apply orig, SkinDef self, GameObject modelObject)
        {
            orig(self, modelObject);
            if (modelObject.name.StartsWith("mdlMerc"))
            {
                SS2Log.Info("self.name: " + self.name);
                if (self.name.Equals("SkinMercenaryVestige"))
                {
                    if (modelObject)
                    {
                        CharacterModel tempmodel = modelObject.GetComponent<CharacterModel>();
                        tempmodel.baseLightInfos[0].defaultColor = new Color(1, 0.302f, 0, 1);
                        tempmodel.baseLightInfos[1].defaultColor = new Color(0.682f, 0.220f, 0.059f, 1);


                        ChildLocator childLocator = modelObject.GetComponent<ChildLocator>();
                        if (childLocator)
                        {
                            Transform PreDashEffect = childLocator.FindChild("PreDashEffect");
                            //PreDashEffect.GetChild(0).GetComponent<ParticleSystem>().startColor = new Color(1f, 0.5613f, 0.6875f, 1); //0.5613 0.6875 1 1 
                            PreDashEffect.GetChild(1).GetComponent<Light>().color = new Color(1f, 0.2f, 0.2f, 1); //0.2028 0.6199 1 1
                            PreDashEffect.GetChild(2).GetComponent<ParticleSystem>().startColor = new Color(1f, 0.5613f, 0.6875f, 1);  //0.5613 0.6875 1 1
                            //PreDashEffect.GetChild(2).GetComponent<ParticleSystemRenderer>().material = RedMercSkin.matMercIgnitionRed; //matMercIgnition (Instance)
                            PreDashEffect.GetChild(3).GetComponent<ParticleSystem>().startColor = new Color(1f, 0.5613f, 0.6875f, 1);  //0.5613 0.6875 1 1 
                            //PreDashEffect.GetChild(3).GetComponent<ParticleSystemRenderer>().material = RedMercSkin.matMercIgnitionRed; //matMercIgnition (Instance)
                        }

                    }
                }
            }
        }
    }
}