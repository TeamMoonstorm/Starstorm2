using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemMercenary
{
    public sealed class NemMercenaryGrandMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.nemmerc.grandmastery", SS2Bundle.NemMercenary);

        public sealed class NemMercenaryGrandMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.5f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("NemMercBody", SS2Bundle.NemMercenary).GetComponent<CharacterBody>();
        }
    }
}