/*using BepInEx.Configuration;

namespace Moonstorm.Starstorm2
{
    public static class StaticValues
    {
        //public static float forkDamageValue;

        //public static float coffeeAttackSpeedValue;
        //public static float coffeeMoveSpeedValue;

        public static float maliceRangeValue;
        public static float maliceRangeStackValue;
        public static float maliceDmgReductionValue;
        public static float maliceProcCoefValue;

        public static float trematodeDamage;
        public static float trematodeDuration;
        public static float trematodeCritical;

        //public static float diaryTime;

        public static float coinChance;
        public static float coinDuration;
        public static float coinDamage;
        public static float coinMoneyGained;

        public static float massFactor;
        public static float massHealthGain;

        //public static float testerGold;
        //public static float testerHealing;

        //public static float gadgetDamage;
        //public static float gadgetCrit;

        public static float droidLife;
        //public static float droidDamage;
        //public static float droidSpeed;

        //public static float soulChance;

        public static float bootsBase;
        public static float bootsStack;
        public static float bootsRadius;
        public static float bootsProc;
        //public static JetBootsEffectQuality timbsQuality;

        public static float canBaseChance;
        //public static float canStackChance;
        //public static float canDuration;
        //public static float canDamage;

        //public static float dungusBase;
        //public static float dungusStack;
        //public static float dungusTime;

        //public static float choccyThreshold;
        //public static float choccyBaseTime;
        //public static float choccyStackTime;

        //public static float hottestSusRadius;
        //public static float hottestSusHit;
        //public static float hottestSusDuration;
        //public static float hottestSusDamage;

        //public static float sekiroArmor;
        //public static float sekiroArmorStack;
        //public static float sekiroCrit;
        //public static float sekiroCritStack;

        internal static void InitValues()
        {
            //sekiroArmor = ItemStatConfigValue("Hunters Sigil", "Armor", "Base armor gained", 15f);
            //sekiroArmorStack = ItemStatConfigValue("Hunters Sigil", "Armor Stacking", "Armor gained per stack", 10f);
            //sekiroCrit = ItemStatConfigValue("Hunters Sigil", "Crit Chance", "Base crit chance", 25f);
            //sekiroCritStack = ItemStatConfigValue("Hunters Sigil", "Crit Stacking", "Crit chance per stack", 20f);

            //hottestSusHit = ItemStatConfigValue("Hottest Sauce", "Damage", "Damage of the initial bursts damage", 1.5f);
            //hottestSusRadius = ItemStatConfigValue("Hottest Sauce", "Radius", "Radius of hottest sauce effects", 30f);
            //hottestSusDuration = ItemStatConfigValue("Hottest Sauce", "Duration", "Duration of hottest sauces burn", 6f);
            //hottestSusDamage = ItemStatConfigValue("Hottest Sauce", "Burn Damage", "Damage multipler of hottest sauces burn", 1f);

            //choccyThreshold = ItemStatConfigValue("Green Chocolate", "Health Threshold", "Amount of health to lose to proc", 0.2f);
            //choccyBaseTime = ItemStatConfigValue("Green Chocolate", "Base Duration", "Duration of the green chocolate buff", 5f);
            //choccyStackTime = ItemStatConfigValue("Green Chocolate", "Duration Stacking", "Duration of the green chocolate buff per green chocolate", 10f);

            //dungusBase = ItemStatConfigValue("Dormant Fungus", "Base Healing", "Base healing per second", 0.015f);
            //dungusStack = ItemStatConfigValue("Dormant Fungus", "Heal Stacking", "Healing per stack of dungus", 0.005f);
            //dungusTime = ItemStatConfigValue("Dormant Fungus", "Heal Timer", "How many seconds between heals", 1f);

            canBaseChance = ItemStatConfigValue("Strange Can", "Base Chance", "Base chance of intoxication", 8.5f);
            //canStackChance = ItemStatConfigValue("Strange Can", "Chance Stacking", "Chance of intoxication per strange can", 5f);
            //canDuration = ItemStatConfigValue("Strange Can", "Duration", "Duration of intoxication effect", 3.5f);
            //canDamage = ItemStatConfigValue("Strange Can", "Damage", "Damage of intoxication effect per tick", 1f);

            bootsBase = ItemStatConfigValue("Prototype Jet Boots", "Damage", "Damage dealt on jump", 1.5f);
            bootsStack = ItemStatConfigValue("Prototype Jet Boots", "Damage Stacking", "Damage added per stack", 1f);
            bootsRadius = ItemStatConfigValue("Prototype Jet Boots", "Radius", "Radius of explosion", 7.5f);
            bootsProc = ItemStatConfigValue("Prototype Jet Boots", "Proc Coefficient", "Proc coefficient", 0f);
            //timbsQuality = Starstorm.instance.Config.Bind("Starstorm 2 :: Items :: Prototype Jet Boots", "Effect Quality", JetBootsEffectQuality.Default, new ConfigDescription("Quality of the explosion effect")).Value;

            //forkDamageValue = ItemStatConfigValue("Fork", "Damage", "Amount of damage per fork", 0.07f);

            //coffeeAttackSpeedValue = ItemStatConfigValue("Coffee Bag", "Attack Speed", "Amount of attack speed per coffee bag", 0.075f);
            //coffeeMoveSpeedValue = ItemStatConfigValue("Coffee Bag", "Movement Speed", "Amount of movement speed per coffee bag", 0.07f);

            maliceRangeValue = ItemStatConfigValue("Malice", "Radius", "Radius", 9.0f);
            maliceRangeStackValue = ItemStatConfigValue("Malice", "Radius Stacking", "Amount of added range per malice", 1f);
            maliceDmgReductionValue = ItemStatConfigValue("Malice", "Damage Preserved", "Amount of damage preserved", 0.55f);
            maliceProcCoefValue = ItemStatConfigValue("Malice", "Proc Coefficient", "Proc coefficient", 0f);

            trematodeDamage = ItemStatConfigValue("Detritive Trematode", "Damage", "Damage per hit", 1f);
            trematodeDuration = ItemStatConfigValue("Detritive Trematode", "Duration", "Duration of trematodes", 3f);
            trematodeCritical = ItemStatConfigValue("Detritive Trematode", "Critical Health Threshold", "Threshold for trematodes to activate", 0.4f);

            //diaryTime = ItemStatConfigValue("Diary", "XP Gain Rate", "Seconds between xp gain", 2f);

            coinChance = ItemStatConfigValue("Molten Coin", "Proc Rate", "Chance to burn on hit", 6f);
            coinDuration = ItemStatConfigValue("Molten Coin", "Duration", "Burn duration", 4f);
            coinDamage = ItemStatConfigValue("Molten Coin", "Damage", "Damage dealt by burn", 1f);
            //I don't know how this fucking works but I'm sure some bored user will figure it out.
            coinMoneyGained = ItemStatConfigValue("Molten Coin", "Money Gained", "Money gained per proc", 1f);

            massFactor = ItemStatConfigValue("Relic of Mass", "Acceleration Multiplier", "Factor by which acceleration is multiplied", 8f);
            massHealthGain = ItemStatConfigValue("Relic of Mass", "Health Multiplier", "Amount of health is gained", 1f);

            //testerGold = ItemStatConfigValue("Broken Blood Tester", "Gold Per Heal", "Amount of gold earned on heal", 5f);
            //testerHealing = ItemStatConfigValue("Broken Blood Tester", "Healing Threshold", "Amount of health to proc", 15f);

            //gadgetDamage = ItemStatConfigValue("Erratic Gadget", "Crit Boost Amount", "Damage added to crit", 0.5f);
            //gadgetCrit = ItemStatConfigValue("Erratic Gadget", "Crit Chance Increase", "Amount of added crit chance", 10f);

            droidLife = ItemStatConfigValue("Droid Head", "Drone Lifetime", "Lifetime of drones", 15f);
            //droidDamage = ItemStatConfigValue("Droid Head", "Drone Damage", "Damage dealt by drones", 1f);
            //droidSpeed = ItemStatConfigValue("Droid Head", "Drone Speed", "Movement speed of drones", 2f);

            //soulChance = ItemStatConfigValue("Stirring Soul", "Item Chance", "Chance of dropping an item", 3f);
        }

        // helper for ez item stat config
        internal static float ItemStatConfigValue(string itemName, string configName, string desc, float defaultValue)
        {
            ConfigEntry<float> config = Starstorm.instance.Config.Bind("Starstorm 2 :: Items :: " + itemName, configName, defaultValue, new ConfigDescription(desc));
            return config.Value;
        }

        internal static float ItemStatStupidConfigValue(string itemName, string configName, string desc, int defaultValue)
        {
            ConfigEntry<float> config = Starstorm.instance.Config.Bind<float>("Starstorm 2 :: Items :: " + itemName, configName, defaultValue, new ConfigDescription(desc));
            return config.Value;
        }

        public enum JetBootsEffectQuality
        {
            None,
            Light,
            Default
        };
    }
}*/