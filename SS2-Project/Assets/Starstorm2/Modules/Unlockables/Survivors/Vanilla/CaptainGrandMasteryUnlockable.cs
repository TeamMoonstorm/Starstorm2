using RoR2;
using UnityEngine;
namespace SS2.Unlocks.VanillaSurvivors
{

    public sealed class CaptainGrandMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.5f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("CaptainBody");
        }
    }

}