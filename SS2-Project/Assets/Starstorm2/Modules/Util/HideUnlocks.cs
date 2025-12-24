using RoR2;
using RoR2.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SS2
{
    public class HideUnlocks
    {
        [SystemInitializer]
        private static void Hook()
        {
            //character unlocks
            On.RoR2.CharacterSelectBarController.Awake += CharacterSelectBarController_Awake;
        }

        private static void CharacterSelectBarController_Awake(On.RoR2.CharacterSelectBarController.orig_Awake orig, CharacterSelectBarController self)
        {
            //hide nems from css proper
            if (self != null && SS2Content.Survivors.NemMerc != null && SS2Content.Survivors.NemMerc.bodyPrefab != null)
                SS2Content.Survivors.NemMerc.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.NemMerc.survivorIndex);

            if (self != null &&  SS2Content.Survivors.survivorNemCommando != null && SS2Content.Survivors.survivorNemCommando.bodyPrefab != null)
                SS2Content.Survivors.survivorNemCommando.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.survivorNemCommando.survivorIndex);

            orig(self);
        }
    }
}
