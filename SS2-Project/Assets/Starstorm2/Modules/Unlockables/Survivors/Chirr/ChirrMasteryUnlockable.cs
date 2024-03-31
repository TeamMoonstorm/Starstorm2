using RoR2;
using UnityEngine;

using Moonstorm;
namespace SS2.Unlocks.Chirr
{
    public sealed class ChirrMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.chirr.mastery", SS2Bundle.Chirr);

        public sealed class ChirrMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.0f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("ChirrBody", SS2Bundle.Chirr).GetComponent<CharacterBody>();
        }
    }
}
