using MSU.Config;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine.Networking;
namespace SS2
{

    //TODO: Create proper difficulty module home-made
    public class SuperTyphoon : SS2Difficulty
    {
        public override SS2AssetRequest<SerializableDifficultyDef> AssetRequest => SS2Assets.LoadAssetAsync<SerializableDifficultyDef>("SuperTyphoon", SS2Bundle.Base);

        private int defMonsterCap;
        public static SerializableDifficultyDef sdd;
        public override void Initialize()
        {
            sdd = DifficultyDef;

            /*
//On.RoR2.UI.MPEventSystemLocator.Awake += MPEventSystemLocator_Awake;

//On.RoR2.UI.RuleBookViewer.Awake += 

/*superTyphoonRCD.ruleDef = superTyphoonRuleDef;
superTyphoonRCD.localName = "SS2_DIFFICULTY_SUPERTYPHOON_NAME";
superTyphoonRCD.globalName = superTyphoonRuleDef.globalName + "." + "SS2_DIFFICULTY_SUPERTYPHOON_NAME";
superTyphoonRCD.extraData = null;
superTyphoonRCD.excludeByDefault = true;
superTyphoonRCD.difficultyIndex = SuperTyphoonIndex;
*/
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnRunEnd(Run run)
        {
            TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Void).softCharacterLimit = defMonsterCap;
            TeamCatalog.GetTeamDef(TeamIndex.Lunar).softCharacterLimit = defMonsterCap;
        }

        public override void OnRunStart(Run run)
        {
            defMonsterCap = TeamCatalog.GetTeamDef(TeamIndex.Monster).softCharacterLimit;

            foreach (CharacterMaster cm in run.userMasters.Values)
                if (NetworkServer.active)
                    cm.inventory.GiveItem(RoR2Content.Items.MonsoonPlayerHelper.itemIndex);
        }

        //Bad Ending
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
    }
}
