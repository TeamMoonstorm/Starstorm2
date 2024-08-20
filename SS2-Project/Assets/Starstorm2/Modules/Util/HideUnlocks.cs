using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2
{
    public class HideUnlocks
    {
        public static void Hook()
        {
            On.RoR2.CharacterSelectBarController.Awake += CharacterSelectBarController_Awake;
        }

        private static void CharacterSelectBarController_Awake(On.RoR2.CharacterSelectBarController.orig_Awake orig, CharacterSelectBarController self)
        {
            //hide nemcommando from css proper
            if (SS2Content.Survivors.NemMerc.bodyPrefab != null)
                SS2Content.Survivors.NemMerc.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.NemMerc.survivorIndex); // hello nem comado

            if (SS2Content.Survivors.survivorNemCommando.bodyPrefab != null)
                SS2Content.Survivors.survivorNemCommando.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.survivorNemCommando.survivorIndex);

            orig(self);
        }
    }
}
