using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Executioner
{
    public sealed class ExecutionerMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.executioner.mastery", SS2Bundle.Executioner);

        /*public override void Initialize()
        {
            AddRequiredType<Survivors.Executioner>();
        }*/

        public sealed class ExecutionerMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.0f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("ExecutionerBody", SS2Bundle.Executioner).GetComponent<CharacterBody>();
        }
    }
}