using RoR2;
using R2API;
using SS2;
using MSU;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class RulebookEnabler : MonoBehaviour
{
    public static RuleDef rule;
    public static RuleCategoryDef ruleCategoryDef;
    private static int ruleCategoryIndex;
    public static bool categoryEnabled = false;
    internal static IEnumerator Init()
    {
        PreGameController.onPreGameControllerSetRuleBookGlobal += RuleBookGlobal;
        PreGameController.onPreGameControllerSetRuleBookServerGlobal += ServerRuleBookGlobal;
        PreGameController.onServerRecalculatedModifierAvailability += onServerRecalculatedModifierAvailability;
        PreGameRuleVoteController.onVotesUpdated += OnVotesUpdated;

        ruleCategoryDef = RuleCatalog.AddCategory("SS2_RULEBOOK_NAME", "SS2_RULEBOOK_DESC", Color.red, "SS2_RULEBOOK_EMPTY", "SS2_RULEBOOK_EDIT", () => categoryEnabled, RuleCatalog.RuleCategoryType.VoteResultGrid);
        ruleCategoryDef.position = 250;

        rule = new RuleDef("ss2TestRuleInternal", "SS2_RULE_TEST_NAME");
        rule.category = ruleCategoryDef;
        rule.defaultChoiceIndex = 0;

        RuleChoiceDef enabledChoice = rule.AddChoice("SS2_RULE_TEST_ENABLED", "Enabled");
        enabledChoice.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Achievements/texCrocoClearGameMonsoonIcon.png").WaitForCompletion();
        enabledChoice.tooltipNameToken = "SS2_RULE_TEST_NAME_TOKEN";
        enabledChoice.tooltipNameColor = Color.white;
        enabledChoice.tooltipBodyToken = "SS2_RULE_TEST_BODY_TOKEN";
        enabledChoice.excludeByDefault = false;
        rule.MakeNewestChoiceDefault();

        RuleChoiceDef disabledChoice = rule.AddChoice("SS2_RULE_TEST_DISABLED", "Disabled");
        disabledChoice.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Achievements/texEngiClearGameMonsoonIcon.png").WaitForCompletion();
        disabledChoice.tooltipNameToken = "SS2_RULE_TEST_DISABLED_NAME_TOKEN";
        disabledChoice.tooltipNameColor = Color.black;
        disabledChoice.tooltipBodyToken = "SS2_RULE_TEST_DISABLED_BODY_TOKEN";
        disabledChoice.excludeByDefault = false;

        RuleChoiceDef thirdThingChoice = rule.AddChoice("SS2_RULE_TEST_THIRDTHIND", "ThirdThing");
        thirdThingChoice.sprite = Addressables.LoadAssetAsync<Sprite>("RoR2/Base/Common/MiscIcons/texRuleMapIsRandom.png").WaitForCompletion();
        thirdThingChoice.tooltipNameToken = "SS2_RULE_TEST_THIRDTHING_NAME_TOKEN";
        thirdThingChoice.tooltipNameColor = Color.green;
        thirdThingChoice.tooltipBodyToken = "SS2_RULE_TEST_THIRDTHING_BODY_TOKEN";
        thirdThingChoice.excludeByDefault = false;


        RuleCatalog.AddRule(rule);
        
        

        yield return null;
    }

    private static void onServerRecalculatedModifierAvailability(RoR2.PreGameController obj)
    {
        FixRules();
    }

    private static void OnVotesUpdated()
    {
        FixRules();
    }

    private static void NetworkRuleBookComponent_onRuleBookUpdated(NetworkRuleBook nrb)
    {
        FixRules();
    }

    private static void ServerRuleBookGlobal(PreGameController preGameController, RuleBook rb)
    {
        preGameController.networkRuleBookComponent.onRuleBookUpdated += NetworkRuleBookComponent_onRuleBookUpdated;
        ExposeRulebook(preGameController.networkRuleBookComponent.ruleBook);
        FixRules();
    }

    private static void RuleBookGlobal(PreGameController preGameController, RuleBook ruleBook)
    {
        if (preGameController != null && preGameController.gameModeIndex == GameModeCatalog.FindGameModeIndex("ClassicRun") || preGameController.gameModeIndex == GameModeCatalog.FindGameModeIndex("InfiniteTowerRun"))
        {
            ExposeRulebook(preGameController.networkRuleBookComponent.ruleBook);
        }
    }

    private static void ExposeRulebook(RuleBook rulebook)
    {
        bool hasUpdatedRules = false;
        foreach (var ruleChoiceDef in rulebook.choices)
        {
            RuleCategoryDef category = ruleChoiceDef.ruleDef.category;
            if (category.displayToken == "RULE_HEADER_MISC" && !hasUpdatedRules)
            {
                foreach (var ruleDef in category.children)
                {
                    ruleDef.category = ruleCategoryDef;
                    foreach (var subRuleChoiceDef in ruleDef.choices)
                    {
                        subRuleChoiceDef.excludeByDefault = false;
                        
                    }
                }
            }

            if (category.displayToken == "RULE_HEADER_DIFFICULTY")
            {
                foreach (var ruleDef in category.children)
                {
                    Debug.Log("ruledef name : " + ruleDef.displayToken);
                }
            }
        }
    }

    private static void FixRules()
    {
        foreach (var choiceController in RoR2.UI.RuleChoiceController.instancesList)
        {
            if (choiceController.choiceDef.ruleDef.choices.Count > 1)
            {
                choiceController.canVote = true;
            }
        }
    }
}
