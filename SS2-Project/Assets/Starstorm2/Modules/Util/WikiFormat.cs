using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using System.IO;
using Path = System.IO.Path;
namespace SS2
{
    public static class WikiFormat
    {
        const string WIKI_OUTPUT_FOLDER = "wiki";
        const string WIKI_OUTPUT_ITEM = "Items.txt";
        static string WikiOutputPath = Path.Combine(Path.GetDirectoryName(SS2Main.Instance.Info.Location), WIKI_OUTPUT_FOLDER);

        [ConCommand(commandName = "ss2wiki_item", flags = ConVarFlags.None, helpText = "Print Starstorm 2 item information to a Wiki.GG format.")]
        public static void FormatItem(ConCommandArgs args)
        {
            string path = Path.Combine(WikiOutputPath, WIKI_OUTPUT_ITEM);
            string f = "[\u201C{0}\u201C] = {{\n\tRarity = \u201C{1}\u201C,\n\tQuote = \u201C{2}\u201C,\n\tDesc = \u201C{3}\u201C,\n\tCategory = {{ \u201C{4}\u201C }},\n\tUnlock = \u201C{5}\u201C,\n\tCorrupt = \u201C{6}\u201C, \n\tUncorrupt = \u201C{7}\u201C,\n\t ID = ,\n\tStats = {{\n\t\t {{\n\t\t\tStat = \u201C\u201C,\n\t\t\tValue = \u201C\u201C,\n\t\t\tStack = \u201C\u201C,\n\t\t\tAdd = \u201C\u201C\n\t\t}}\n\t}},\n\tLocalizationInternalName = \u201C{8}\u201C\n\t}}";
            if (!Directory.Exists(WikiOutputPath))
            {
                Directory.CreateDirectory(WikiOutputPath);
            }
            TextWriter tw = new StreamWriter(path, false);
            foreach (ItemDef item in SS2Assets.LoadAllAssets<ItemDef>(SS2Bundle.All))
            {
                string itemName = Language.GetString(item.nameToken);
                ItemTier itemTier = item.tier;
                string rarity = itemTier switch
                {
                    ItemTier.Tier1 => "Common",
                    ItemTier.Tier2 => "Uncommon",
                    ItemTier.Tier3 => "Legendary",
                    ItemTier.Lunar => "Lunar",
                    ItemTier.Boss => "Boss",
                    ItemTier.VoidTier1 => "Void",
                    ItemTier.VoidTier2 => "Void",
                    ItemTier.VoidTier3 => "Void",
                    ItemTier.VoidBoss => "Void",
                    ItemTier.NoTier => "Untiered",
                    _ => "Untiered",
                };
                if (itemTier == SS2Content.ItemTierDefs.Sibylline.tier)
                    rarity = "Sybilline";
                string pickup = Language.GetString(item.pickupToken);
                string desc = Language.GetString(item.descriptionToken);
                string tags = "";
                for(int i = 0; i < item.tags.Length; i++)
                {
                    tags += Enum.GetName(typeof(ItemTag), item.tags[i]);
                    if (i < item.tags.Length - 1) tags += ", ";
                }
                string unlock = "";
                if (item.unlockableDef)
                    unlock = Language.GetString(AchievementManager.GetAchievementDefFromUnlockable(item.unlockableDef.cachedName)?.nameToken);
                string token = item.nameToken;
                
                string format = Language.GetStringFormatted(f, itemName, rarity, pickup, desc, tags, unlock, String.Empty, String.Empty, token);
                tw.WriteLine(format);           
            }
            tw.Close();
        }
    }
}
