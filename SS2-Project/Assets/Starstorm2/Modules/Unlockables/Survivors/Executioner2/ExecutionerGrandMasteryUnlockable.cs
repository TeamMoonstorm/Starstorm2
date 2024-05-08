using RoR2;
using UnityEngine;
namespace SS2.Unlocks.Executioner2
{
    public sealed class Executioner2GrandMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.5f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("Executioner2Body");
        }
    }
    
}