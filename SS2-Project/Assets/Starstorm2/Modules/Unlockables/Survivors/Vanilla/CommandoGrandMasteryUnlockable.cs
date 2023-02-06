using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.VanillaSurvivors
{
    public sealed class CommandoGrandMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.commando.grandmastery", SS2Bundle.Vanilla);

        public sealed class CommandoGrandMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.5f;

            public override CharacterBody RequiredCharacterBody { get; set; } = Resources.Load<GameObject>("prefabs/characterbodies/commandobody").GetComponent<CharacterBody>();
        }
    }
}