using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.VanillaSurvivors
{
    public sealed class HuntressAltSkin1Achievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("HuntressBody");
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

        private class HuntressAltSkin1ServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                EntityStates.CrystalPickup.DestroyCrystal.onPickup += OnCrystalGrabbed;
            }

            public override void OnUninstall()
            {
                EntityStates.CrystalPickup.DestroyCrystal.onPickup -= OnCrystalGrabbed;
                base.OnUninstall();
            }

            private void OnCrystalGrabbed(GameObject obj)
            {
                SkinCrystal scc = obj.GetComponent<SkinCrystal>();
                if (scc != null)
                {
                    //check that the player is the right body, and the unlock matches the crystal's id
                    if (scc.bodyIndex == BodyCatalog.FindBodyIndex("HuntressBody") && scc.skinUnlockID == 1)
                    {
                        Grant();
                    }
                }
            }
        }
    }
    
}