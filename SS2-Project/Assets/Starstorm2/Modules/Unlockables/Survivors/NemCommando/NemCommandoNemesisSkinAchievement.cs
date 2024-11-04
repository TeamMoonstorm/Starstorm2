using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCommando
{
    public sealed class NemCommandoNemesisSkinAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemCommandoBody");
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
        private class NemCommandoNemesisSkinServerAchievement : BaseServerAchievement
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
                // if nemado = nemado
                if (obj.bodyIndex == networkUser.GetCurrentBody().bodyIndex)
                {
                    Grant();
                }
            }

        }
    }    
}