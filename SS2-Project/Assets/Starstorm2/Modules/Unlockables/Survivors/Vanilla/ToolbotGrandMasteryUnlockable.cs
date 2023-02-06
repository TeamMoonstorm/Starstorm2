using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.VanillaSurvivors
{
    public sealed class ToolbotGrandMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.toolbot.grandmastery", SS2Bundle.Vanilla);

        public sealed class ToolbotGrandMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.5f;

            public override CharacterBody RequiredCharacterBody { get; set; } = Resources.Load<GameObject>("prefabs/characterbodies/toolbotbody").GetComponent<CharacterBody>();
        }
    }
}