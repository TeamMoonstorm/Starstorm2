using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Unlocks.Executioner
{
    public sealed class ExecutionerGrandMasteryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.skin.executioner.grandmastery", SS2Bundle.Executioner);

        /*public override void Initialize()
        {
            AddRequiredType<Survivors.Executioner>();
        }*/

        public sealed class ExecutionerGrandMasteryAchievement : GenericMasteryAchievement
        {
            public override float RequiredDifficultyCoefficient { get; set; } = 3.5f;

            public override CharacterBody RequiredCharacterBody { get; set; } = SS2Assets.LoadAsset<GameObject>("ExecutionerBody", SS2Bundle.Executioner).GetComponent<CharacterBody>();
        }
    }
}