using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemMercenary
{

    public sealed class NemMercenaryKnifeAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemMercBody");
        }
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedServer += OnNemMercenaryDefeated;
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            EntityStates.Events.GenericNemesisEvent.onNemesisDefeatedServer -= OnNemMercenaryDefeated;
        }

        private void OnNemMercenaryDefeated(CharacterBody obj)
        {
            if (obj.bodyIndex != BodyCatalog.FindBodyIndex("NemMercBody"))
            {
                Grant();
            }
        }

    }
    
}