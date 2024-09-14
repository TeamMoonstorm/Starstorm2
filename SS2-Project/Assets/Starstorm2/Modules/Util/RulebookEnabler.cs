﻿using RoR2;
using R2API;
using SS2;
using MSU;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using RoR2.UI;

public class RulebookEnabler : MonoBehaviour
{
    //adding your own rules i guess-
    //define a rule, give it this rulecategorydef v
    public static RuleCategoryDef ruleCategoryDef;
    //look below for how rules are defined and how they're hooked. its done on scenestart for savemod purposes.

    //player dealt damage rule
    public static RuleDef playerDamageRule;
    public static int playerDamageRuleIndex;
    public static float playerDamageModifier;

    //player taken damage rule. 'armor' maybe misleading. whatever.
    public static RuleDef playerArmorRule;
    public static int playerArmorRuleIndex;
    public static float playerArmorModifier;

    //orb just grab the bools here when you grab them.
    //you might cry if you scroll down.

    //storm toggle rule
    public static RuleDef stormsEnabledRule;
    public static int stormsEnabledRuleIndex;
    public static bool stormsEnabled;

    //event toggle rule
    public static RuleDef eventsEnabledRule;
    public static int eventsEnabledRuleIndex;
    public static bool eventsEnabled;

    //ss elites toggle rule
    public static RuleDef starstormElitesEnabledRule;
    public static int starstormElitesEnabledRuleIndex;
    public static bool starstormElitesEnabled;

    //call me 195 the amount of times ive written 'rule'
    public static Color color = new Color(0.52f, 0.74f, 0.87f, 1);
    private static int ruleCategoryIndex;
    public static bool categoryEnabled = false;
    internal static IEnumerator Init()
    {
        On.RoR2.UI.RuleCategoryController.SetData += OnRoR2UIRuleCategoryControllerSetData;
        On.RoR2.SceneDirector.Start += SceneDirector_Start;
        On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;

        ruleCategoryDef = new RuleCategoryDef();

        ruleCategoryDef.displayToken = "SS2_RULEBOOK_NAME";
        ruleCategoryDef.subtitleToken = "SS2_RULEBOOK_DESC";
        ruleCategoryDef.color = color;
        ruleCategoryDef.emptyTipToken = "SS2_RULEBOOK_EMPTY";
        ruleCategoryDef.editToken = "SS2_RULEBOOK_EDIT";
        ruleCategoryDef.ruleCategoryType = RuleCatalog.RuleCategoryType.VoteResultGrid;
        ruleCategoryDef.position = 250;
        ruleCategoryIndex = RuleCatalogExtras.AddCategory(ruleCategoryDef);

        //Player Damage Dealt Modifier
        {
            playerDamageRule = new RuleDef("ss2PlayerDamageMod", "SS2_PLAYER_DAMAGE_RULE_NAME");
            playerDamageRule.category = ruleCategoryDef;

            ExtendedRuleChoiceDef defaultDamageChoice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRuleDefault", "DamageDefault");
            defaultDamageChoice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageDefault", SS2Bundle.Base);
            defaultDamageChoice.tooltipNameToken = "SS2_PLAYER_DAMAGE_DEFAULT_NAME_TOKEN";
            defaultDamageChoice.tooltipNameColor = color;
            defaultDamageChoice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_DEFAULT_BODY_TOKEN";
            defaultDamageChoice.excludeByDefault = false;
            playerDamageRule.MakeNewestChoiceDefault();

            ExtendedRuleChoiceDef increaseDamage25Choice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRule25Increase", "25DamageIncrease");
            increaseDamage25Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageIncrease25", SS2Bundle.Base);
            increaseDamage25Choice.tooltipNameToken = "SS2_PLAYER_DAMAGE_25_INCREASE_NAME_TOKEN";
            increaseDamage25Choice.tooltipNameColor = color;
            increaseDamage25Choice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_25_INCREASE_BODY_TOKEN";
            increaseDamage25Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef increaseDamage50Choice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRule50Increase", "50DamageIncrease");
            increaseDamage50Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageIncrease50", SS2Bundle.Base);
            increaseDamage50Choice.tooltipNameToken = "SS2_PLAYER_DAMAGE_50_INCREASE_NAME_TOKEN";
            increaseDamage50Choice.tooltipNameColor = color;
            increaseDamage50Choice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_50_INCREASE_BODY_TOKEN";
            increaseDamage50Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef increaseDamage75Choice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRule75Increase", "75DamageIncrease");
            increaseDamage75Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageIncrease75", SS2Bundle.Base);
            increaseDamage75Choice.tooltipNameToken = "SS2_PLAYER_DAMAGE_75_INCREASE_NAME_TOKEN";
            increaseDamage75Choice.tooltipNameColor = color;
            increaseDamage75Choice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_75_INCREASE_BODY_TOKEN";
            increaseDamage75Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef increaseDamage100Choice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRule100Increase", "100DamageIncrease");
            increaseDamage100Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageIncrease100", SS2Bundle.Base);
            increaseDamage100Choice.tooltipNameToken = "SS2_PLAYER_DAMAGE_100_INCREASE_NAME_TOKEN";
            increaseDamage100Choice.tooltipNameColor = color;
            increaseDamage100Choice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_100_INCREASE_BODY_TOKEN";
            increaseDamage100Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef decreaseDamage25Choice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRule25Decrease", "25DamageDecrease");
            decreaseDamage25Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageDecrease25", SS2Bundle.Base);
            decreaseDamage25Choice.tooltipNameToken = "SS2_PLAYER_DAMAGE_25_DECREASE_NAME_TOKEN";
            decreaseDamage25Choice.tooltipNameColor = color;
            decreaseDamage25Choice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_25_DECREASE_BODY_TOKEN";
            decreaseDamage25Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef decreaseDamage50Choice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRule50Decrease", "50DamageDecrease");
            decreaseDamage50Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageDecrease50", SS2Bundle.Base);
            decreaseDamage50Choice.tooltipNameToken = "SS2_PLAYER_DAMAGE_50_DECREASE_NAME_TOKEN";
            decreaseDamage50Choice.tooltipNameColor = color;
            decreaseDamage50Choice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_50_DECREASE_BODY_TOKEN";
            decreaseDamage50Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef decreaseDamage75Choice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRule75Decrease", "75DamageDecrease");
            decreaseDamage75Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageDecrease75", SS2Bundle.Base);
            decreaseDamage75Choice.tooltipNameToken = "SS2_PLAYER_DAMAGE_75_DECREASE_NAME_TOKEN";
            decreaseDamage75Choice.tooltipNameColor = color;
            decreaseDamage75Choice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_75_DECREASE_BODY_TOKEN";
            decreaseDamage75Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef decreaseDamage90Choice = playerDamageRule.AddExtendedRuleChoiceDef("ss2PlayerDamageRule90Decrease", "90DamageDecrease");
            decreaseDamage90Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleDamageDecrease90", SS2Bundle.Base);
            decreaseDamage90Choice.tooltipNameToken = "SS2_PLAYER_DAMAGE_90_DECREASE_NAME_TOKEN";
            decreaseDamage90Choice.tooltipNameColor = color;
            decreaseDamage90Choice.tooltipBodyToken = "SS2_PLAYER_DAMAGE_90_DECREASE_BODY_TOKEN";
            decreaseDamage90Choice.excludeByDefault = false;

            RuleCatalogExtras.AddRuleToCatalog(playerDamageRule, ruleCategoryIndex);
            playerDamageRuleIndex = playerDamageRule.globalIndex;
        }

        //Player Damage Taken Modifier
        {
            playerArmorRule = new RuleDef("ss2PlayerArmorMod", "SS2_PLAYER_ARMOR_RULE_NAME");
            playerArmorRule.category = ruleCategoryDef;

            //misleading choice names but whatever
            ExtendedRuleChoiceDef defaultArmorChoice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRuleDefault", "Default");
            defaultArmorChoice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorDefault", SS2Bundle.Base);
            defaultArmorChoice.tooltipNameToken = "SS2_PLAYER_ARMOR_DEFAULT_NAME_TOKEN";
            defaultArmorChoice.tooltipNameColor = color;
            defaultArmorChoice.tooltipBodyToken = "SS2_PLAYER_ARMOR_DEFAULT_BODY_TOKEN";
            defaultArmorChoice.excludeByDefault = false;
            playerArmorRule.MakeNewestChoiceDefault();

            ExtendedRuleChoiceDef increaseArmor25Choice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRule25Increase", "25ArmorIncrease");
            increaseArmor25Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorIncrease25", SS2Bundle.Base);
            increaseArmor25Choice.tooltipNameToken = "SS2_PLAYER_ARMOR_25_INCREASE_NAME_TOKEN";
            increaseArmor25Choice.tooltipNameColor = color;
            increaseArmor25Choice.tooltipBodyToken = "SS2_PLAYER_ARMOR_25_INCREASE_BODY_TOKEN";
            increaseArmor25Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef increaseArmor50Choice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRule50Increase", "50ArmorIncrease");
            increaseArmor50Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorIncrease50", SS2Bundle.Base);
            increaseArmor50Choice.tooltipNameToken = "SS2_PLAYER_ARMOR_50_INCREASE_NAME_TOKEN";
            increaseArmor50Choice.tooltipNameColor = color;
            increaseArmor50Choice.tooltipBodyToken = "SS2_PLAYER_ARMOR_50_INCREASE_BODY_TOKEN";
            increaseArmor50Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef increaseArmor75Choice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRule75Increase", "75ArmorIncrease");
            increaseArmor75Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorIncrease75", SS2Bundle.Base);
            increaseArmor75Choice.tooltipNameToken = "SS2_PLAYER_ARMOR_75_INCREASE_NAME_TOKEN";
            increaseArmor75Choice.tooltipNameColor = color;
            increaseArmor75Choice.tooltipBodyToken = "SS2_PLAYER_ARMOR_75_INCREASE_BODY_TOKEN";
            increaseArmor75Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef increaseArmor100Choice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRule100Increase", "100ArmorIncrease");
            increaseArmor100Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorIncrease100", SS2Bundle.Base);
            increaseArmor100Choice.tooltipNameToken = "SS2_PLAYER_ARMOR_100_INCREASE_NAME_TOKEN";
            increaseArmor100Choice.tooltipNameColor = color;
            increaseArmor100Choice.tooltipBodyToken = "SS2_PLAYER_ARMOR_100_INCREASE_BODY_TOKEN";
            increaseArmor100Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef decreaseArmor25Choice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRule25Decrease", "25ArmorDecrease");
            decreaseArmor25Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorDecrease25", SS2Bundle.Base);
            decreaseArmor25Choice.tooltipNameToken = "SS2_PLAYER_ARMOR_25_DECREASE_NAME_TOKEN";
            decreaseArmor25Choice.tooltipNameColor = color;
            decreaseArmor25Choice.tooltipBodyToken = "SS2_PLAYER_ARMOR_25_DECREASE_BODY_TOKEN";
            decreaseArmor25Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef decreaseArmor50Choice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRule50Decrease", "50ArmorDecrease");
            decreaseArmor50Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorDecrease50", SS2Bundle.Base);
            decreaseArmor50Choice.tooltipNameToken = "SS2_PLAYER_ARMOR_50_DECREASE_NAME_TOKEN";
            decreaseArmor50Choice.tooltipNameColor = color;
            decreaseArmor50Choice.tooltipBodyToken = "SS2_PLAYER_ARMOR_50_DECREASE_BODY_TOKEN";
            decreaseArmor50Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef decreaseArmor75Choice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRule75Decrease", "75ArmorDecrease");
            decreaseArmor75Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorDecrease75", SS2Bundle.Base);
            decreaseArmor75Choice.tooltipNameToken = "SS2_PLAYER_ARMOR_75_DECREASE_NAME_TOKEN";
            decreaseArmor75Choice.tooltipNameColor = color;
            decreaseArmor75Choice.tooltipBodyToken = "SS2_PLAYER_ARMOR_75_DECREASE_BODY_TOKEN";
            decreaseArmor75Choice.excludeByDefault = false;

            ExtendedRuleChoiceDef decreaseArmor90Choice = playerArmorRule.AddExtendedRuleChoiceDef("ss2PlayerArmorRule90Decrease", "90ArmorDecrease");
            decreaseArmor90Choice.sprite = SS2Assets.LoadAsset<Sprite>("texIconRuleArmorDecrease90", SS2Bundle.Base);
            decreaseArmor90Choice.tooltipNameToken = "SS2_PLAYER_ARMOR_90_DECREASE_NAME_TOKEN";
            decreaseArmor90Choice.tooltipNameColor = color;
            decreaseArmor90Choice.tooltipBodyToken = "SS2_PLAYER_ARMOR_90_DECREASE_BODY_TOKEN";
            decreaseArmor90Choice.excludeByDefault = false;

            RuleCatalogExtras.AddRuleToCatalog(playerArmorRule, ruleCategoryIndex);
            playerArmorRuleIndex = playerArmorRule.globalIndex;
        }

        //Starstorm Storms Toggle
        {
            stormsEnabledRule = new RuleDef("ss2StormsEnabled", "SS2_STORMS_ENABLED_RULE_NAME");
            stormsEnabledRule.category = ruleCategoryDef;

            ExtendedRuleChoiceDef stormsEnabled = stormsEnabledRule.AddExtendedRuleChoiceDef("ss2StormsEnabled", "StormsEnabled");
            stormsEnabled.sprite = null;
            stormsEnabled.tooltipNameToken = "SS2_STORMS_ENABLED_NAME_TOKEN";
            stormsEnabled.tooltipNameColor = color;
            stormsEnabled.tooltipBodyToken = "SS2_STORMS_ENABLED_BODY_TOKEN";
            stormsEnabled.excludeByDefault = false;
            stormsEnabledRule.MakeNewestChoiceDefault();

            ExtendedRuleChoiceDef stormsDisabled = stormsEnabledRule.AddExtendedRuleChoiceDef("ss2StormsDisabled", "StormsDisabled");
            stormsDisabled.sprite = null;
            stormsDisabled.tooltipNameToken = "SS2_STORMS_DISABLED_NAME_TOKEN";
            stormsDisabled.tooltipNameColor = color;
            stormsDisabled.tooltipBodyToken = "SS2_STORMS_DISABLED_BODY_TOKEN";
            stormsDisabled.excludeByDefault = false;

            RuleCatalogExtras.AddRuleToCatalog(stormsEnabledRule, ruleCategoryIndex);
            stormsEnabledRuleIndex = stormsEnabledRule.globalIndex;
        }

        //Starstorm Event Toggle
        {
            eventsEnabledRule = new RuleDef("ss2EventsEnabled", "SS2_EVENTS_ENABLED_RULE_NAME");
            eventsEnabledRule.category = ruleCategoryDef;

            ExtendedRuleChoiceDef eventsEnabled = eventsEnabledRule.AddExtendedRuleChoiceDef("ss2EventsEnabled", "EventsEnabled");
            eventsEnabled.sprite = null;
            eventsEnabled.tooltipNameToken = "SS2_EVENTS_ENABLED_NAME_TOKEN";
            eventsEnabled.tooltipNameColor = color;
            eventsEnabled.tooltipBodyToken = "SS2_EVENTS_ENABLED_BODY_TOKEN";
            eventsEnabled.excludeByDefault = false;
            eventsEnabledRule.MakeNewestChoiceDefault();

            ExtendedRuleChoiceDef eventsDisabled = eventsEnabledRule.AddExtendedRuleChoiceDef("ss2EventsDisabled", "EventsDisabled");
            eventsDisabled.sprite = null;
            eventsDisabled.tooltipNameToken = "SS2_EVENTS_DISABLED_NAME_TOKEN";
            eventsDisabled.tooltipNameColor = color;
            eventsDisabled.tooltipBodyToken = "SS2_EVENTS_DISABLED_BODY_TOKEN";
            eventsDisabled.excludeByDefault = false;

            RuleCatalogExtras.AddRuleToCatalog(eventsEnabledRule, ruleCategoryIndex);
            eventsEnabledRuleIndex = eventsEnabledRule.globalIndex;
        }

        //Starstorm Elite Toggle
        {
            starstormElitesEnabledRule = new RuleDef("ss2StarstormElitesEnabled", "SS2_STARSTORM_ELITES_ENABLED_RULE_NAME");
            starstormElitesEnabledRule.category = ruleCategoryDef;

            ExtendedRuleChoiceDef starstormElitesEnabled = starstormElitesEnabledRule.AddExtendedRuleChoiceDef("ss2StarstormElitesEnabled", "StarstormElitesEnabled");
            starstormElitesEnabled.sprite = null;
            starstormElitesEnabled.tooltipNameToken = "SS2_STARSTORM_ELITES_ENABLED_NAME_TOKEN";
            starstormElitesEnabled.tooltipNameColor = color;
            starstormElitesEnabled.tooltipBodyToken = "SS2_STARSTORM_ELITES_ENABLED_BODY_TOKEN";
            starstormElitesEnabled.excludeByDefault = false;
            starstormElitesEnabledRule.MakeNewestChoiceDefault();

            ExtendedRuleChoiceDef starstormElitesDisabled = starstormElitesEnabledRule.AddExtendedRuleChoiceDef("ss2StarstormElitesDisabled", "StarstormElitesDisabled");
            starstormElitesDisabled.sprite = null;
            starstormElitesDisabled.tooltipNameToken = "SS2_STARSTORM_ELITES_DISABLED_NAME_TOKEN";
            starstormElitesDisabled.tooltipNameColor = color;
            starstormElitesDisabled.tooltipBodyToken = "SS2_STARSTORM_ELITES_DISABLED_BODY_TOKEN";
            starstormElitesDisabled.excludeByDefault = false;

            RuleCatalogExtras.AddRuleToCatalog(starstormElitesEnabledRule, ruleCategoryIndex);
            starstormElitesEnabledRuleIndex = starstormElitesEnabledRule.globalIndex;
        }


        yield return null;
    }

    public static void OnRoR2UIRuleCategoryControllerSetData(On.RoR2.UI.RuleCategoryController.orig_SetData orig, RuleCategoryController self, RuleCategoryDef categoryDef, RuleChoiceMask availability, RuleBook rulebook)
    {
        orig(self, categoryDef, availability, rulebook);
        if (categoryDef == ruleCategoryDef)
        {
            for (int i = 0; i < self.rulesToDisplay.Count; i++)
            {
                self.popoutButtonIconAllocator.elements[i].canVote = true;
            }
        }
    }

    private static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
    {
        if (damageInfo.attacker != null)
        {
            CharacterBody attackerBody = damageInfo.attacker.transform.GetComponentInChildren<CharacterBody>();
            if (attackerBody != null)
            {
                if (attackerBody.isPlayerControlled)
                {
                    damageInfo.damage *= playerDamageModifier;
                }
            }
        }

        if (self.body != null)
        {
            if (self.body.isPlayerControlled)
            {
                damageInfo.damage *= playerArmorModifier;
            }
        }

        orig(self, damageInfo);
    }

    //these are checked on stage start because savemod. assuming these are saved by that (they should be..?)
    public static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
    {
        orig(self);

        //muerde
        foreach (RuleChoiceDef rcd in Run.instance.ruleBook.choices)
        {
            //Damage Dealt Rule
            if (rcd.ruleDef == playerDamageRule)
            {
                switch (rcd.tooltipNameToken)
                {
                    case "SS2_PLAYER_DAMAGE_25_INCREASE_NAME_TOKEN":    //something something 'token never  changes'
                        playerDamageModifier = 1.25f;                   //just feeling lazy this time
                        break;                                          //but should probably be rulechoicedef index...
                    case "SS2_PLAYER_DAMAGE_50_INCREASE_NAME_TOKEN":
                        playerDamageModifier = 1.5f;
                        break;
                    case "SS2_PLAYER_DAMAGE_75_INCREASE_NAME_TOKEN":
                        playerDamageModifier = 1.75f;
                        break;
                    case "SS2_PLAYER_DAMAGE_100_INCREASE_NAME_TOKEN":
                        playerDamageModifier = 2f;
                        break;
                    case "SS2_PLAYER_DAMAGE_25_DECREASE_NAME_TOKEN":
                        playerDamageModifier = 0.75f;
                        break;
                    case "SS2_PLAYER_DAMAGE_50_DECREASE_NAME_TOKEN":
                        playerDamageModifier = 0.5f;
                        break;
                    case "SS2_PLAYER_DAMAGE_75_DECREASE_NAME_TOKEN":
                        playerDamageModifier = 0.25f;
                        break;
                    case "SS2_PLAYER_DAMAGE_90_DECREASE_NAME_TOKEN":
                        playerDamageModifier = 0.1f;
                        break;
                    case "SS2_PLAYER_DAMAGE_DEFAULT_NAME_TOKEN":
                        playerDamageModifier = 1f;
                        break;
                    default:
                        playerDamageModifier = 1f;
                        break;
                }
            }

            //Damage Taken Rule
            if (rcd.ruleDef == playerArmorRule)
            {
                switch (rcd.tooltipNameToken)
                {
                    case "SS2_PLAYER_ARMOR_25_INCREASE_NAME_TOKEN":
                        playerArmorModifier = 1.25f;
                        break;
                    case "SS2_PLAYER_ARMOR_50_INCREASE_NAME_TOKEN":
                        playerArmorModifier = 1.5f;
                        break;
                    case "SS2_PLAYER_ARMOR_75_INCREASE_NAME_TOKEN":
                        playerArmorModifier = 1.75f;
                        break;
                    case "SS2_PLAYER_ARMOR_100_INCREASE_NAME_TOKEN":
                        playerArmorModifier = 2f;
                        break;
                    case "SS2_PLAYER_ARMOR_25_DECREASE_NAME_TOKEN":
                        playerArmorModifier = 0.75f;
                        break;
                    case "SS2_PLAYER_ARMOR_50_DECREASE_NAME_TOKEN":
                        playerArmorModifier = 0.5f;
                        break;
                    case "SS2_PLAYER_ARMOR_75_DECREASE_NAME_TOKEN":
                        playerArmorModifier = 0.25f;
                        break;
                    case "SS2_PLAYER_ARMOR_90_DECREASE_NAME_TOKEN":
                        playerArmorModifier = 0.1f;
                        break;
                    case "SS2_PLAYER_ARMOR_DEFAULT_NAME_TOKEN":
                        playerArmorModifier = 1f;
                        break;
                    default:
                        playerArmorModifier = 1f;
                        break;
                }
            }

            //Storms Enabled Rule
            if (rcd.ruleDef == stormsEnabledRule)
            {
                if (rcd.tooltipNameToken == "SS2_STORMS_DISABLED_NAME")
                    stormsEnabled = false;

                else
                    stormsEnabled = true;
            }

            //Events Enabled Rule
            if (rcd.ruleDef == eventsEnabledRule)
            {
                if (rcd.tooltipNameToken == "SS2_EVENTS_DISABLED_NAME")
                    eventsEnabled = false;

                else
                    eventsEnabled = true;
            }

            //Starstorm Elites Enabled Rule
            if (rcd.ruleDef == starstormElitesEnabledRule)
            {
                if (rcd.tooltipNameToken == "SS2_STARSTORM_ELITES_DISABLED_NAME")
                    starstormElitesEnabled = false;

                else
                    starstormElitesEnabled = true;
            }
        }
    }
}
