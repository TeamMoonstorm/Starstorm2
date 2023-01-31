using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Nemmando
{
    public sealed class NemmandoMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.nemmando.mastery", SS2Bundle.Nemmando);

        /*public override void Initialize()
        {
            AddRequiredType<Survivors.Nemmando>();
        }*/

        public sealed class NemmandoMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.0f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("NemmandoBody", SS2Bundle.Nemmando).GetComponent<CharacterBody>();
        }
    }
}
