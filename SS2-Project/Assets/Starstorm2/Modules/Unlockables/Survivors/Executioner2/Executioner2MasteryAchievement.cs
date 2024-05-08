using RoR2;
using UnityEngine;
namespace SS2.Unlocks.Executioner2
{
    public sealed class Executioner2MasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.0f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("Executioner2Body");
        }
    }   
}