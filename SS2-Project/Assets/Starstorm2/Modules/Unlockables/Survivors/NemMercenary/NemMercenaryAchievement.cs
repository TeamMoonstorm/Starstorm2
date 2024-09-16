using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryAchievement : BaseAchievement
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

        // TODO: fix whenevr events get done
        private class NemMercenaryUnlockableServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemMercenaryDefeated;
            }

            public override void OnUninstall()
            {
                EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal -= OnNemMercenaryDefeated;
                base.OnUninstall();
            }

            private void OnNemMercenaryDefeated(CharacterBody obj)
            {
                if (obj.bodyIndex == BodyCatalog.FindBodyIndex("NemMercBody"))
                {
                    Grant();
                }
            }
        }
    }   
}