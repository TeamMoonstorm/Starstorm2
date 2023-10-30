using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryCloneUnlock : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skill.nemmercenary.clone", SS2Bundle.NemMercenary);

        public sealed class NemMercenaryCloneAchievement : BaseAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                SS2Log.Warning("NemMercenaryCloneUnlock");
                SetServerTracked(true);
            }

            public override void OnUninstall()
            {
                base.OnUninstall();
            }

            private class NemMercenaryCloneServerAchievement : BaseServerAchievement
            {
                public BodyIndex nemMercBodyIndex
                {
                    get
                    {
                        var nemMercBodyPrefab = SS2Assets.LoadAsset<GameObject>("NemMercBody", SS2Bundle.NemMercenary);
                        if (nemMercBodyPrefab)
                        {
                            return nemMercBodyPrefab.GetComponent<CharacterBody>().bodyIndex;
                        }
                        return BodyIndex.None;
                    }
                }

                
                private void Check(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
                {
                    orig(self, activator);

                    if (self.displayNameToken.Equals("SS2_DRONE_CLONE_INTERACTABLE_NAME")) // idk how else to do it other than adding an event to the interactable prefab (annoying)
                    {
                        CharacterBody body = activator.GetComponent<CharacterBody>();
                        if (body && body.bodyIndex == nemMercBodyIndex && networkUser.GetCurrentBody() == body)
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
}