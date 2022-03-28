//What even is this? looks like rulebook unlocker? - Nebby.
/*
using RoR2;
using System;

namespace Moonstorm.Starstorm2
{
    class RuleBook
    {
        private static bool ConfigItemCatalog()
        {
            return !Config.preRunItemCatalog.Value;
        }
        private static bool ConfigEquipmentCatalog()
        {
            return !Config.preRunEquipmentCatalog.Value;
        }

        private static bool ReturnFalse()
        {
            return false;
        }
        private static bool ReturnTrue()
        {
            return true;
        }

        public static void Initialize()
        {
            //RoR2.Run.onServerRunSetRuleBookGlobal += Run_onServerRunSetRuleBookGlobal;
            //RoR2.Run.onRunSetRuleBookGlobal += Run_onRunSetRuleBookGlobal;

            PreGameController.onPreGameControllerSetRuleBookGlobal += PreGameController_onPreGameControllerSetRuleBookGlobal;
            PreGameController.onPreGameControllerSetRuleBookServerGlobal += PreGameController_onPreGameControllerSetRuleBookServerGlobal;
        }

        private static void PreGameController_onPreGameControllerSetRuleBookServerGlobal(RoR2.PreGameController arg1, RoR2.RuleBook arg2)
        {
            ChangeRuleCatalogRuleCategoryDef();
        }

        private static void PreGameController_onPreGameControllerSetRuleBookGlobal(RoR2.PreGameController arg1, RoR2.RuleBook arg2)
        {
            ChangeRuleCatalogRuleCategoryDef();
        }

        private static void Run_onRunSetRuleBookGlobal(RoR2.Run arg1, RoR2.RuleBook arg2)
        {
        }

        private static void Run_onServerRunSetRuleBookGlobal(RoR2.Run arg1, RoR2.RuleBook arg2)
        {
        }

        private static void ChangeRuleCatalogRuleCategoryDef()
        {

            //RoR2.RuleCatalog.GetCategoryDef(3).hiddenTest = new Func<bool>(HiddenTestItemsConvar);
            foreach (var categoryDef in RuleCatalog.allCategoryDefs)
            {
                //categoryDef.hiddenTest = new Func<bool>(ReturnFalse);
                if (categoryDef.displayToken == "RULE_HEADER_ITEMS")
                {
                    categoryDef.hiddenTest = new Func<bool>(ConfigItemCatalog);
                }
                if (categoryDef.displayToken == "RULE_HEADER_EQUIPMENT")
                {
                    categoryDef.hiddenTest = new Func<bool>(ConfigEquipmentCatalog);
                }
            }
        }
    }
}
*/