using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using System;
using MSU;
using System.Collections;
using MSU.Config;
using System.Collections.Generic;
using Assets.Starstorm2;
using static R2API.DamageAPI;
using R2API;
using RoR2.ContentManagement;

namespace SS2.Survivors
{
    public sealed class Chirr : SS2Survivor
    {
        public override SS2AssetRequest<SurvivorAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SurvivorAssetCollection>("acChirr", SS2Bundle.Chirr);

        public static Vector3 chirristmasPos = new Vector3(-6.8455f, -7.0516f, 57.0163f);
        public static Vector3 chirristmasRot = new Vector3(0, 178.3926f, 0);

        private static GameObject chirristmas;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR, configDescOverride = "Can Weightless Frame be activated by toggling.")]
        public static bool toggleHover = false;
        
        public static float confuseDuration = 4f;
        public static ModdedDamageType confuseDamageType;
        public static float _confuseSlowAmount = 0.5f;
        public static float _confuseAttackSpeedSlowAmount = 0.0f;

        private float _convertDotDamageCoefficient = 0.8f;
        public static DotController.DotIndex ConvertDotIndex { get; private set; }

        private static float _grabFriendAttackBoost = 1f;

        private static float _percentHealthRegen = 0.05f;


        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void Initialize()
        {
            if (SS2Main.ScepterInstalled)
            {
                ScepterCompat();
            }

            if (SS2Main.ChristmasTime)
            {
                On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += HiChirrHiiiiii;
            }

            Stage.onStageStartGlobal += FixGoolakeRaycasts;

            ModifyPrefab();
            R2API.RecalculateStatsAPI.GetStatCoefficients += ModifyStats;

            RegisterConfuseOnHit();
            RegisterConvert();

            Material _matFriendOverlay = AssetCollection.FindAsset<Material>("matFriendOverlay");
            BuffDef friendBuffDef = AssetCollection.FindAsset<BuffDef>("BuffChirrFriend");
            BuffOverlays.AddBuffOverlay(friendBuffDef, _matFriendOverlay);
        }    

        private void ModifyPrefab()
        {
            var cb = CharacterPrefab.GetComponent<CharacterBody>();

            // would be cool to have something unique for her
            // someone mentioned her "hatching" from a tree i think
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");

            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }

        // This is used for Chirr appearing on the menu during the holiday event
        private void HiChirrHiiiiii(On.RoR2.UI.MainMenu.BaseMainMenuScreen.orig_OnEnter orig, RoR2.UI.MainMenu.BaseMainMenuScreen self, RoR2.UI.MainMenu.MainMenuController mainMenuController)
        {
            orig(self, mainMenuController);
            if (chirristmas) return;
            chirristmas = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("ChirrDisplay", SS2Bundle.Chirr), chirristmasPos, Quaternion.Euler(chirristmasRot));
            chirristmas.transform.localScale = Vector3.one * 2.4f;
        }

        //Disables CookForFasterSimulation on the terrain in goolake, since it fucks up world raycasts
        // only when chirr is in the stage cuz idk how badly it affects performance
        private void FixGoolakeRaycasts(Stage stage)
        {
            BodyIndex chirr = CharacterPrefab.GetComponent<CharacterBody>().bodyIndex;
            if (stage.sceneDef == SceneCatalog.GetSceneDefFromSceneName("goolake"))
            {
                foreach (PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                {
                    if (pcmc.master.bodyPrefab.GetComponent<CharacterBody>().bodyIndex == chirr)
                    {
                        GameObject terrain = GameObject.Find("HOLDER: GameplaySpace/Terrain");
                        if (terrain)
                        {
                            SS2Log.Warning("Player Chirr found. Disabling terrain mesh optimization on goolake to avoid gameplay bugs.");
                            terrain.GetComponent<MeshCollider>().cookingOptions &= ~MeshColliderCookingOptions.CookForFasterSimulation;
                            break;
                        }
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
        public void ScepterCompat()
        {
            AncientScepter.AncientScepterItem.instance.RegisterScepterSkill(SS2Assets.LoadAsset<SkillDef>("BefriendScepter", SS2Bundle.Chirr), "ChirrBody", SkillSlot.Special, 0);
        }

        private void ModifyStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasBuff(SS2Content.Buffs.BuffChirrConfuse))
            {
                args.moveSpeedMultAdd -= _confuseSlowAmount;
                args.attackSpeedMultAdd -= _confuseAttackSpeedSlowAmount;
            }         
            if(sender.HasBuff(SS2Content.Buffs.BuffChirrGrabFriend))
            {
                args.baseAttackSpeedAdd += _grabFriendAttackBoost;
            }         
            if(sender.HasBuff(SS2Content.Buffs.BuffChirrRegen))
            {
                args.baseRegenAdd += sender.maxHealth * _percentHealthRegen;
            }
        }

        
    
        private void RegisterConfuseOnHit()
        {
            confuseDamageType = R2API.DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += ApplyConfuse;
        }

        private void RegisterConvert()
        {
            ConvertDotIndex = DotAPI.RegisterDotDef(0.33f, _convertDotDamageCoefficient, DamageColorIndex.Poison, AssetCollection.FindAsset<BuffDef>("BuffChirrConvert"));
        }
        private void ApplyConfuse(DamageReport obj)
        {
            var victimBody = obj.victimBody;
            var damageInfo = obj.damageInfo;
            if (DamageAPI.HasModdedDamageType(damageInfo, confuseDamageType))
            {
                victimBody.AddTimedBuffAuthority(SS2Content.Buffs.BuffChirrConfuse.buffIndex, confuseDuration);
            }
        }

        
    }
}
