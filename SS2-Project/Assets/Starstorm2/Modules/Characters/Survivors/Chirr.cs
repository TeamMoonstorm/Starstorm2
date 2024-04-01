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

namespace SS2.Survivors
{
    public sealed class Chirr : SS2Survivor
    {
        public override SurvivorDef SurvivorDef => _survivorDef;
        private SurvivorDef _survivorDef;
        public override NullableRef<GameObject> MasterPrefab => _monsterMaster;
        private GameObject _monsterMaster;
        public override GameObject CharacterPrefab => _prefab;
        private GameObject _prefab;

        public static Vector3 chirristmasPos = new Vector3(-6.8455f, -7.0516f, 57.0163f);
        public static Vector3 chirristmasRot = new Vector3(0, 178.3926f, 0);

        private static GameObject chirristmas;

        [RiskOfOptionsConfigureField(SS2Config.ID_SURVIVOR, ConfigDescOverride = "Can Weightless Frame be activated by toggling.")]
        public static bool toggleHover = false;
        
        public static float confuseDuration = 4f;

        public static ModdedDamageType confuseDamageType;

        public override void Initialize()
        {
            if (SS2Main.ScepterInstalled)
            {
                ScepterCompat();
            }

            DateTime today = DateTime.Today;
            if (today.Month == 12 && ((today.Day == 27) || (today.Day == 26) || (today.Day == 25) || (today.Day == 24) || (today.Day == 23)))
            {
                On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += HiChirrHiiiiii;
            }

            Stage.onStageStartGlobal += FixGoolakeRaycasts;

            ModifyPrefab();
            RegisterConfuseOnHit();
        }

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
            BodyIndex chirr = _prefab.GetComponent<CharacterBody>().bodyIndex;
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

        public override bool IsAvailable()
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            ParallelAssetLoadCoroutineHelper helper = new ParallelAssetLoadCoroutineHelper();

            helper.AddAssetToLoad<GameObject>("ChirrBody", SS2Bundle.Chirr);
            helper.AddAssetToLoad<GameObject>("ChirrMonsterMaster", SS2Bundle.Chirr);
            helper.AddAssetToLoad<SurvivorDef>("Chirr", SS2Bundle.Chirr);

            helper.Start();
            while (!helper.IsDone()) yield return null;

            _prefab = helper.GetLoadedAsset<GameObject>("ChirrBody");
            _monsterMaster = helper.GetLoadedAsset<GameObject>("ChirrMonsterMaster");
            _survivorDef = helper.GetLoadedAsset<SurvivorDef>("Chirr");
        }

        private IEnumerator LoadAndAssign<T>(string assetName, SS2Bundle bundle, Dictionary<string, UnityEngine.Object> dictionary) where T : UnityEngine.Object
        {
            var request = SS2Assets.LoadAssetAsync<T>(assetName, bundle);
            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            dictionary.Add(assetName, request.Asset);
        }

        private void ModifyPrefab()
        {
            var cb = _prefab.GetComponent<CharacterBody>();

            // would be cool to have something unique for her
            // someone mentioned her "hatching" from a tree i think
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod");

            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }
    
        private void RegisterConfuseOnHit()
        {
            confuseDamageType = R2API.DamageAPI.ReserveDamageType();
            GlobalEventManager.onServerDamageDealt += ApplyConfuse;
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
