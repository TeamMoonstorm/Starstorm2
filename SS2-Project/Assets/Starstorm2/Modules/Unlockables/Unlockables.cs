using RoR2;
using System.Collections.Generic;

namespace SS2
{
    public static class UnlockAllHandler
    {
        private static HashSet<UnlockableDef> ss2Unlockables;

        internal static void Init()
        {
            if (!SS2Config.unlockAll.value)
                return;

            UnlockableCatalog.availability.onAvailable += BuildCacheAndHook;
        }

        private static void BuildCacheAndHook()
        {
            ss2Unlockables = new HashSet<UnlockableDef>();

            var unlockables = SS2Content.SS2ContentPack.unlockableDefs;
            for (int i = 0; i < unlockables.Length; i++)
            {
                UnlockableDef def = unlockables[i];
                if (def == null) continue;

                // skip skins we want players to still earn those
                if (def.cachedName.IndexOf("skin", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;

                ss2Unlockables.Add(def);
            }

            On.RoR2.UserProfile.HasUnlockable_UnlockableDef += ForceHasUnlockable;
            SS2Log.Info($"UnlockAllHandler: Forcing {ss2Unlockables.Count} SS2 unlockables as unlocked (skins excluded).");
        }

        private static bool ForceHasUnlockable(On.RoR2.UserProfile.orig_HasUnlockable_UnlockableDef orig, UserProfile self, UnlockableDef unlockableDef)
        {
            if (unlockableDef != null && ss2Unlockables.Contains(unlockableDef))
                return true;

            return orig(self, unlockableDef);
        }
    }
}
