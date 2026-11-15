using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCroco
{
    public sealed class NemCrocoNemesisSkinAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemCrocoBody");
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
        private class NemCrocoNemesisSkinServerAchievement : BaseServerAchievement
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
                if (obj.bodyIndex == networkUser.GetCurrentBody().bodyIndex)
                {
                    Grant();
                }
            }

        }
    }    
}