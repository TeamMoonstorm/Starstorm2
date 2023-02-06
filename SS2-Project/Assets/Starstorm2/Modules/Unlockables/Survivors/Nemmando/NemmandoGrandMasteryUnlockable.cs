using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Nemmando
{
    public sealed class NemmandoGrandMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.nemmando.grandmastery", SS2Bundle.Nemmando);

        public sealed class NemmandoGrandMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.5f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("NemmandoBody", SS2Bundle.Nemmando).GetComponent<CharacterBody>();
        }
    }
}