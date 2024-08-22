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
        public static UnlockableDef[] unlockableDefs = SS2Assets.LoadAllAssets<UnlockableDef>(SS2Bundle.All);
        public static void Hook()
        {
            //character unlocks
            On.RoR2.CharacterSelectBarController.Awake += CharacterSelectBarController_Awake;

            //skins/skills
            On.RoR2.UI.LoadoutPanelController.Row.AddButton += Row_AddButton;
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

        private static void Row_AddButton(On.RoR2.UI.LoadoutPanelController.Row.orig_AddButton orig, object self, LoadoutPanelController owner, Sprite icon, string titleToken, string bodyToken, Color tooltipColor, UnityEngine.Events.UnityAction callback, string unlockableName, ViewablesCatalog.Node viewableNode, bool isWIP)
        {
            foreach (UnlockableDef ud in unlockableDefs)
            {
                //the implication of this is that any skin with the token below is automatically hidden
                if (unlockableName == ud.nameToken && unlockableName.Contains("SS2_ACHIEVEMENT_RECOLOR"))
                {
                    bool unlocked = LocalUserManager.readOnlyLocalUsersList.Any((LocalUser localUser) => localUser.userProfile.HasUnlockable(ud));
                    if (!unlocked) return;
                }
            }
            
            orig(self, owner, icon, titleToken, bodyToken, tooltipColor, callback, unlockableName, viewableNode, isWIP);
        }
    }
}
