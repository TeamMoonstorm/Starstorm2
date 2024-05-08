using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.VanillaSurvivors
{
    public sealed class CommandoNemesisSkinAchievement : BaseAchievement
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

        // TODO: fix whenever events get done
        private class CommandoNemesisSkinServerAchievement : BaseServerAchievement
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