using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemExecutioner
{
    public sealed class NemExecutionerNemesisSkinAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemExecutionerBody");
        }

        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            base.SetServerTracked(true);
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            base.SetServerTracked(false);
        }

        // TODO: fix this whenever events get done
        private class NemExecutionerNemesisSkinServerAchievement : BaseServerAchievement
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
                // if nemexo = nemexo
                if (obj.bodyIndex == networkUser.GetCurrentBody().bodyIndex)
                {
                    Grant();
                }
            }

        }
    }    
}