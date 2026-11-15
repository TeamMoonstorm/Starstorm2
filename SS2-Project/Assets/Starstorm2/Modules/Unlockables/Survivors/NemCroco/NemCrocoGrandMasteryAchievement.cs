using RoR2;
using UnityEngine;
namespace SS2.Unlocks.NemCroco
{
    public sealed class NemCrocoGrandMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.5f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemCrocoBody");
        }
    }
    
}