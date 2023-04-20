using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.NemCommando
{
    public sealed class NemCommandoMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.nemcommando.mastery", SS2Bundle.NemCommando);

        /*public override void Initialize()
        {
            AddRequiredType<Survivors.Nemmando>();
        }*/

        public sealed class NemCommandoMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.0f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("NemCommandoBody", SS2Bundle.NemCommando).GetComponent<CharacterBody>();
        }
    }
}
