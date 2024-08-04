using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RulebookEnabler : MonoBehaviour
{
    internal static IEnumerator Init()
    {
        RoR2.PreGameController.onPreGameControllerSetRuleBookGlobal += RuleBookGlobal;
        RoR2.PreGameController.onPreGameControllerSetRuleBookServerGlobal += ServerRuleBookGlobal;
        RoR2.PreGameController.onServerRecalculatedModifierAvailability += onServerRecalculatedModifierAvailability;
        RoR2.PreGameRuleVoteController.onVotesUpdated += OnVotesUpdated;

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

    private static void NetworkRuleBookComponent_onRuleBookUpdated(RoR2.NetworkRuleBook nrb)
    {
        FixRules();
    }

    private static void ServerRuleBookGlobal(RoR2.PreGameController pgc, RoR2.RuleBook rb)
    {
        pgc.networkRuleBookComponent.onRuleBookUpdated += NetworkRuleBookComponent_onRuleBookUpdated;
        ExposeRulebook(pgc.networkRuleBookComponent.ruleBook);
        FixRules();
    }

    private static void RuleBookGlobal(RoR2.PreGameController pgc, RoR2.RuleBook rb)
    {
        if (pgc != null && pgc.gameModeIndex == RoR2.GameModeCatalog.FindGameModeIndex("ClassicRun") || pgc.gameModeIndex == RoR2.GameModeCatalog.FindGameModeIndex("InfiniteTowerRun"))
        {
            ExposeRulebook(pgc.networkRuleBookComponent.ruleBook);
        }
    }

    private static void ExposeRulebook(RoR2.RuleBook rulebook)
    {
        foreach (var rcd in rulebook.choices)
        {
            RoR2.RuleCategoryDef cat = rcd.ruleDef.category;
            if (cat.displayToken == "RULE_HEADER_MISC")
            {
                foreach (var rd in cat.children)
                {
                    foreach (var srcd in rd.choices)
                    {
                        srcd.excludeByDefault = false;
                    }
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
