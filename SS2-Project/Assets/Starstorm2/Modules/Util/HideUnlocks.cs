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
        public static UnlockableDef[] unlockableDefs;
        public static void Hook()
        {
            //character unlocks
            On.RoR2.CharacterSelectBarController.Awake += CharacterSelectBarController_Awake;

            unlockableDefs = SS2Assets.LoadAllAssets<UnlockableDef>(SS2Bundle.All);

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
            //the implication of this is that any skin with the tokens below are automatically hidden if not unlocked
            if (titleToken.Contains("SS2_SKIN") && titleToken.Contains("ALT"))
            {
                foreach (UnlockableDef ud in SS2Assets.LoadAllAssets<UnlockableDef>(SS2Bundle.Vanilla))
                {
                    Debug.Log("unlockabledef " + ud.nameToken);
                    Debug.Log("unlockable " + titleToken);

                    //★ OMFG I HATE IT
                    if (titleToken == ud.nameToken)
                    {

                        bool unlocked = LocalUserManager.readOnlyLocalUsersList.Any((LocalUser localUser) => localUser.userProfile.HasUnlockable(ud));
                        if (!unlocked) return;
                    }
                }
            }
            orig(self, owner, icon, titleToken, bodyToken, tooltipColor, callback, unlockableName, viewableNode, isWIP);
        }
    }
}
