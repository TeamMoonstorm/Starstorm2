using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.VanillaSurvivors
{
    public sealed class CommandoAltSkin5Achievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("CommandoBody");
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
        private class CommandoAltSkin5ServerAchievement : BaseServerAchievement
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
                    if (scc.bodyIndex == BodyCatalog.FindBodyIndex("CommandoBody") && scc.skinUnlockID == 5)
                    {
                        Grant();
                    }
                }
            }
        }
    }
    
}