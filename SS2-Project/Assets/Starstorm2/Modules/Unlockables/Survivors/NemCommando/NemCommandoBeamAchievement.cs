using RoR2;
using RoR2.Achievements;
using UnityEngine;
namespace SS2.Unlocks.NemCommando
{  
    public sealed class NemCommandoBeamAchievement : BaseAchievement
    {
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            RoR2Application.onUpdate += CheckBleedChance;
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            RoR2Application.onUpdate -= CheckBleedChance;
        }

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemCommandoBody");
        }

        private void CheckBleedChance()
        {
            if (localUser != null && localUser.cachedBody && localUser.cachedBody.bleedChance >= 100f)
            {
                Grant();
            }
        }
    }        
}