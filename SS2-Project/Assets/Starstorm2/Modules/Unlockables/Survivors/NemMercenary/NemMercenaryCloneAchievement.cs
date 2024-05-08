using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryCloneAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemMercBody");
        }
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            SetServerTracked(true);
        }

        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            SetServerTracked(false);
        }

        private class NemMercenaryCloneServerAchievement : BaseServerAchievement
        {             
            private void Check(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
            {
                orig(self, activator);

                if (self.displayNameToken.Equals("SS2_DRONE_CLONE_INTERACTABLE_NAME")) // idk how else to do it other than adding an event to the interactable prefab (annoying)
                {
                    CharacterBody body = activator.GetComponent<CharacterBody>();
                    if (body && networkUser.GetCurrentBody() == body)
                    {
                        Grant();
                    }
                }                     
            }

            public override void OnInstall()
            {
                base.OnInstall();
                On.RoR2.PurchaseInteraction.OnInteractionBegin += Check;
            }
            public override void OnUninstall()
            {
                base.OnUninstall();
                On.RoR2.PurchaseInteraction.OnInteractionBegin -= Check;
            }

                
        }
    }
    
}