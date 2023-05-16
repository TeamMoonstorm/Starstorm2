using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemCommando
{
    public sealed class NemCommandoGrandMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.nemcommando.grandmastery", SS2Bundle.NemCommando);

        public sealed class NemCommandoGrandMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.5f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("NemCommandoBody", SS2Bundle.NemCommando).GetComponent<CharacterBody>();
        }
    }
}