using RoR2;
using UnityEngine;
namespace SS2.Unlocks.Chirr
{
    public sealed class ChirrMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.0f;
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("ChirrBody");
        }

    }
    
}
