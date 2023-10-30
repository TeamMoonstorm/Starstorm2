using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Executioner2
{
    public sealed class Executioner2GrandMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.executioner2.grandmastery", SS2Bundle.Executioner2);

        public sealed class Executioner2GrandMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.5f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("Executioner2Body", SS2Bundle.Executioner2).GetComponent<CharacterBody>();
        }
    }
}