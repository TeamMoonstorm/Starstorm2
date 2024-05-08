using RoR2;
using UnityEngine;
namespace SS2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.0f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemMercBody");
        }
    }
  
}
