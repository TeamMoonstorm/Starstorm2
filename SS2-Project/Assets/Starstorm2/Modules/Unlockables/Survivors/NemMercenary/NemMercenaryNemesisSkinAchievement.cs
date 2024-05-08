using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryNemesisSkinAchievement : BaseAchievement
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

        private class NemMercenaryNemesisSkinServerAchievement : BaseServerAchievement
        {

            // TODO: fix this whenever events get done
            public override void OnInstall()
            {
                base.OnInstall();
                //EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal += OnNemMercenaryDefeated;
            }

            public override void OnUninstall()
            {
                //EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedGlobal -= OnNemMercenaryDefeated;
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