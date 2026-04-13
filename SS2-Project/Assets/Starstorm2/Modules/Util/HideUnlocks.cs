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
            //skins/skills
            On.RoR2.UI.LoadoutPanelController.Row.AddButton += Row_AddButton;
        }

        private static void Row_AddButton(On.RoR2.UI.LoadoutPanelController.Row.orig_AddButton orig, object self, LoadoutPanelController owner, Sprite icon, string titleToken, string bodyToken, Color tooltipColor, UnityEngine.Events.UnityAction callback, string unlockableName, ViewablesCatalog.Node viewableNode, bool isWIP, int defIndex)
        {
            //the implication of this is that any skin with the tokens below are automatically hidden if not unlocked
            //so dont name your shit "ss2_skin" "recolor" unless its a skin crystal recolor.....
            if (titleToken.Contains("SS2_SKIN") && titleToken.Contains("RECOLOR"))
            {
                foreach (UnlockableDef ud in SS2Assets.LoadAllAssets<UnlockableDef>(SS2Bundle.Vanilla))
                {
                    //★ ugh
                    if (icon == ud.achievementIcon)
                    {

                        bool unlocked = LocalUserManager.readOnlyLocalUsersList.Any((LocalUser localUser) => localUser.userProfile.HasUnlockable(ud));
                        if (!unlocked) return;
                    }
                }
            }

            orig(self, owner, icon, titleToken, bodyToken, tooltipColor, callback, unlockableName, viewableNode, isWIP, defIndex);
        }

        // hide nemesis survivors from CSS, if not unlocked
        private static void CharacterSelectBarController_Awake(On.RoR2.CharacterSelectBarController.orig_Awake orig, CharacterSelectBarController self)
        {
            if (SS2Content.Survivors.NemMerc.bodyPrefab != null)
                SS2Content.Survivors.NemMerc.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.NemMerc.survivorIndex); // hello nem comado

            if (SS2Content.Survivors.survivorNemCommando.bodyPrefab != null)
                SS2Content.Survivors.survivorNemCommando.hidden = !SurvivorCatalog.SurvivorIsUnlockedOnThisClient(SS2Content.Survivors.survivorNemCommando.survivorIndex);

            orig(self);
        }
    }
}