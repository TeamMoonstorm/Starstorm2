using RoR2;
using UnityEngine;
namespace SS2.Unlocks.NemExecutioner
{
    public sealed class NemExecutionerMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.0f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemExecutionerBody");
        }
    }
    
}
