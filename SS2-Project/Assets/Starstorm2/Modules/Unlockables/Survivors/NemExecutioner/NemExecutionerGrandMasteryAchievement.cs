using RoR2;
using UnityEngine;
namespace SS2.Unlocks.NemExecutioner
{
    public sealed class NemExecutionerGrandMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.5f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemExecutionerBody");
        }
    }
    
}