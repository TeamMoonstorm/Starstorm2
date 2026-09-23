using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCroco
{
    public sealed class NemCrocoAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            base.SetServerTracked(true);
        }

        public override void OnUninstall()
        {
            base.OnUninstall();
        }

        private class NemCrocoUnlockableServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemCrocoDefeated;
            }

            public override void OnUninstall()
            {
                EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal -= OnNemCrocoDefeated;
                base.OnUninstall();
            }

            private void OnNemCrocoDefeated(CharacterBody obj)
            {
                if (obj.bodyIndex == BodyCatalog.FindBodyIndex("NemCrocoBody"))
                {
                    Grant();
                }
            }
        }
    }
    
}