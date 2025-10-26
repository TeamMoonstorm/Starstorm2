using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemExecutioner
{
    public sealed class NemExecutionerAchievement : BaseAchievement
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

        private class NemExecutionerUnlockableServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemExecutionerDefeated;
            }

            public override void OnUninstall()
            {
                EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal -= OnNemExecutionerDefeated;
                base.OnUninstall();
            }

            private void OnNemExecutionerDefeated(CharacterBody obj)
            {
                if (obj.bodyIndex == BodyCatalog.FindBodyIndex("NemExecutionerBody"))
                {
                    Grant();
                }
            }
        }
    }
    
}