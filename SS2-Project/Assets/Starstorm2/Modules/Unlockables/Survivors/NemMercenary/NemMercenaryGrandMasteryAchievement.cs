using RoR2;
using UnityEngine;
namespace SS2.Unlocks.NemMercenary
{

    public sealed class NemMercenaryGrandMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.5f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemMercBody");
        }
    }
    
}