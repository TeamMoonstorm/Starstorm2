using R2API;
using R2API.Utils;
using RoR2;
using RoR2.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2
{
    public static class SuperTyphoon
    {
        public static R2API.ScriptableObjects.SerializableDifficultyDef SuperTyphoonDef { get; private set; }
        public static DifficultyIndex SuperTyphoonIndex { get => SuperTyphoonDef.DifficultyIndex; }

        private static int defMonsterCap;

        [RooConfigurableField(SS2Config.IDMain, ConfigSection = "Super Typhoon", ConfigName = "Increase Team Limit", ConfigDesc = "Multiplies the Monster, Lunar, and Void Team maximum size by 3 when enabled. May affect performance.")]
        internal static bool IncreaseSpawnCapST = true;

        private static RuleChoiceDef rcd;

        internal static void Init()
        {
            SuperTyphoonDef = SS2Assets.LoadAsset<R2API.ScriptableObjects.SerializableDifficultyDef>("SuperTyphoon", SS2Bundle.Base);
            //SuperTyphoonDef.hideFromDifficultySelection = true; // THANK YOU NEBBY
            DifficultyAPI.AddDifficulty(SuperTyphoonDef);
            Run.onRunStartGlobal += Run_onRunStartGlobal;
            Run.onRunDestroyGlobal += Run_onRunDestroyGlobal;

            //On.RoR2.UI.MPEventSystemLocator.Awake += MPEventSystemLocator_Awake;

            //On.RoR2.UI.RuleBookViewer.Awake += 

            /*superTyphoonRCD.ruleDef = superTyphoonRuleDef;
            superTyphoonRCD.localName = "SS2_DIFFICULTY_SUPERTYPHOON_NAME";
            superTyphoonRCD.globalName = superTyphoonRuleDef.globalName + "." + "SS2_DIFFICULTY_SUPERTYPHOON_NAME";
            superTyphoonRCD.extraData = null;
            superTyphoonRCD.excludeByDefault = true;
            superTyphoonRCD.difficultyIndex = SuperTyphoonIndex;*/
        }

        //bad ending
        /*private static void MPEventSystemLocator_Awake(On.RoR2.UI.MPEventSystemLocator.orig_Awake orig, MPEventSystemLocator self)
        {
            Debug.Log("I EXIST!!! IM REAL!!!!");
            if (self.gameObject.name == "Choice (Difficulty.Super Typhoon)" || self.gameObject.name.Contains("Super Typhoon") || self.gameObject.GetComponent<RuleChoiceController>() != null)
            {
                Debug.Log("stage 2");
                Debug.Log(self.gameObject.GetComponent<RuleChoiceController>().image.sprite + " - my sprite");
                if (self.gameObject.GetComponent<RuleChoiceController>() != null && self.gameObject.GetComponent<RuleChoiceController>().image.GetComponent<Image>().sprite == SuperTyphoonDef.iconSprite)
                {
                    Debug.Log("I'm the Super Typhoon Icon and I'm going to fucking die tonight :slight_smile:");
                    //Object.Destroy(self.gameObject);
                    self.gameObject.SetActive(false);
                    //this is some stupid janky bullshit but it works oh well.
                }
            }
            orig(self);
        }*/

        private static void Run_onRunStartGlobal(Run run)
        {
            defMonsterCap = TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit;
            if (run.selectedDifficulty == SuperTyphoonIndex)
            {
                foreach (CharacterMaster cm in run.userMasters.Values)
                    if (NetworkServer.active)
                        cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
                if (IncreaseSpawnCapST)
                {
                    TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit *= 3;
                    TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit *= 3;
                    TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit *= 3;
                }
            }
        }

        private static void Run_onRunDestroyGlobal(Run run)
        {
            TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit = defMonsterCap;
        }
    }
}
