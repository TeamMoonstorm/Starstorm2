using RoR2;
using UnityEngine;
using RoR2.Skills;
using System.Runtime.CompilerServices;
using UnityEngine.AddressableAssets;
using R2API;
using System;
namespace Moonstorm.Starstorm2.Survivors
{
    //[DisabledContent]
    public sealed class Chirr : SurvivorBase
    {
        public override GameObject BodyPrefab { get; } = SS2Assets.LoadAsset<GameObject>("ChirrBody", SS2Bundle.Chirr);
        public override GameObject MasterPrefab { get; } = SS2Assets.LoadAsset<GameObject>("ChirrMonsterMaster", SS2Bundle.Chirr);
        public override SurvivorDef SurvivorDef { get; } = SS2Assets.LoadAsset<SurvivorDef>("Chirr", SS2Bundle.Chirr);

        public static Vector3 chirristmasPos = new Vector3(-6.8455f, -7.0516f, 57.0163f);
        public static Vector3 chirristmasRot = new Vector3(0, 178.3926f, 0);

        private static GameObject chirristmas;
        public override void Initialize()
        {
            base.Initialize();
            if (Starstorm.ScepterInstalled)
            {
                ScepterCompat();
            }

            DateTime today = DateTime.Today;
            if (today.Month == 12 && ((today.Day == 27) || (today.Day == 26) || (today.Day == 25) || (today.Day == 24) || (today.Day == 23)))
            {
                On.RoR2.UI.MainMenu.BaseMainMenuScreen.OnEnter += HiChirrHiiiiii;
            }

            Stage.onStageStartGlobal += FixGoolakeRaycasts;
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
            BodyIndex chirr = BodyPrefab.GetComponent<CharacterBody>().bodyIndex;
            if(stage.sceneDef == SceneCatalog.GetSceneDefFromSceneName("goolake"))
            {
                foreach(PlayerCharacterMasterController pcmc in PlayerCharacterMasterController.instances)
                {
                    if(pcmc.master.bodyPrefab.GetComponent<CharacterBody>().bodyIndex == chirr)
                    {
                        GameObject terrain = GameObject.Find("HOLDER: GameplaySpace/Terrain");
                        if(terrain)
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
        public override void ModifyPrefab()
        {
            var cb = BodyPrefab.GetComponent<CharacterBody>();

            // would be cool to have something unique for her
            // someone mentioned her "hatching" from a tree i think
            cb.preferredPodPrefab = Resources.Load<GameObject>("Prefabs/NetworkedObjects/SurvivorPod"); 
             
            cb._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/StandardCrosshair.prefab").WaitForCompletion();
            cb.GetComponent<ModelLocator>().modelTransform.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
        }
    }
}
