using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCommando
{
    public sealed class NemCommandoAchievement : BaseAchievement
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

        private class NemCommandoUnlockableServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemCommandoDefeated;
            }

            public override void OnUninstall()
            {
                EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal -= OnNemCommandoDefeated;
                base.OnUninstall();
            }

            private void OnNemCommandoDefeated(CharacterBody obj)
            {
                if (obj.bodyIndex == BodyCatalog.FindBodyIndex("NemCommandoBody"))
                {
                    Grant();
                }
            }
        }
    }
    
}