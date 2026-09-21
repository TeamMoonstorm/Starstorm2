using RoR2;
using System.Collections.Generic;

namespace SS2
{
    public static class UnlockAllHandler
    {
        private static HashSet<UnlockableDef> ss2Unlockables;

        internal static void Init()
        {
            SS2Log.Info($"UnlockAllHandler.Init() called. unlockAll value = {SS2Config.unlockAll.value}");

            if (!SS2Config.unlockAll.value)
                return;

            SS2Log.Info("UnlockAllHandler: Installing hooks...");
            On.RoR2.UserProfile.HasUnlockable_UnlockableDef += ForceHasUnlockable;
            On.RoR2.UserProfile.HasSurvivorUnlocked += ForceHasSurvivorUnlocked;
        }

        private static void BuildCache()
        {
            ss2Unlockables = new HashSet<UnlockableDef>();

            var contentPack = SS2Content.SS2ContentPack;

            SS2Log.Info($"UnlockAllHandler.BuildCache: itemDefs={contentPack.itemDefs.Length}, equipmentDefs={contentPack.equipmentDefs.Length}, survivorDefs={contentPack.survivorDefs.Length}, skillDefs={contentPack.skillDefs.Length}, artifactDefs={contentPack.artifactDefs.Length}, unlockableDefs={contentPack.unlockableDefs.Length}");

            for (int i = 0; i < contentPack.itemDefs.Length; i++)
            {
                var def = contentPack.itemDefs[i];
                if (def && def.unlockableDef)
                    ss2Unlockables.Add(def.unlockableDef);
            }
            for (int i = 0; i < contentPack.equipmentDefs.Length; i++)
            {
                var def = contentPack.equipmentDefs[i];
                if (def && def.unlockableDef)
                    ss2Unlockables.Add(def.unlockableDef);
            }
            for (int i = 0; i < contentPack.survivorDefs.Length; i++)
            {
                var def = contentPack.survivorDefs[i];
                if (def && def.unlockableDef)
                {
                    SS2Log.Info($"  Survivor '{def.cachedName}' has unlockableDef '{def.unlockableDef.cachedName}'");
                    ss2Unlockables.Add(def.unlockableDef);
                }
            }
            // TODO: Have UnlockAll work for Skills too
            // This is what happens when I code without Unity installed and then push this to my computer with Unity
            //for (int i = 0; i < contentPack.skillDefs.Length; i++)
            //{
            //    var def = contentPack.skillDefs[i];
            //    if (def && def.unlockableDef)
            //        ss2Unlockables.Add(def.unlockableDef);
            //}
            for (int i = 0; i < contentPack.artifactDefs.Length; i++)
            {
                var def = contentPack.artifactDefs[i];
                if (def && def.unlockableDef)
                    ss2Unlockables.Add(def.unlockableDef);
            }

            for (int i = 0; i < contentPack.unlockableDefs.Length; i++)
            {
                var def = contentPack.unlockableDefs[i];
                if (def == null) continue;
                if (def.cachedName.IndexOf("skin", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    continue;
                ss2Unlockables.Add(def);
            }

            SS2Log.Info($"UnlockAllHandler: Built cache with {ss2Unlockables.Count} SS2 unlockables (skins excluded).");
        }

        private static bool ForceHasUnlockable(On.RoR2.UserProfile.orig_HasUnlockable_UnlockableDef orig, UserProfile self, UnlockableDef unlockableDef)
        {
            if (unlockableDef != null)
            {
                if (ss2Unlockables == null)
                    BuildCache();

                if (ss2Unlockables.Contains(unlockableDef))
                    return true;
            }
            return orig(self, unlockableDef);
        }

        private static bool ForceHasSurvivorUnlocked(On.RoR2.UserProfile.orig_HasSurvivorUnlocked orig, UserProfile self, SurvivorIndex survivorIndex)
        {
            SurvivorDef survivorDef = SurvivorCatalog.GetSurvivorDef(survivorIndex);
            if (survivorDef != null && survivorDef.unlockableDef != null)
            {
                if (ss2Unlockables == null)
                    BuildCache();

                if (ss2Unlockables.Contains(survivorDef.unlockableDef))
                    return true;
            }
            return orig(self, survivorIndex);
        }
    }
}
