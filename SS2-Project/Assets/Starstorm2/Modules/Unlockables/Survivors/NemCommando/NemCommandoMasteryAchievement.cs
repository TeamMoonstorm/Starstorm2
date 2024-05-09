using RoR2;
using UnityEngine;
namespace SS2.Unlocks.NemCommando
{
    public sealed class NemCommandoMasteryAchievement : GenericMasteryAchievement
    {
        public override float RequiredDifficultyCoefficient => 3.0f;

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("NemCommandoBody");
        }
    }
    
}
