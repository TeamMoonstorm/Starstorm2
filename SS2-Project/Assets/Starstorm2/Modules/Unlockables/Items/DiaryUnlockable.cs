using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.Pickups
{
    public sealed class DiaryUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.item.diary", SS2Bundle.Items);

        public override void Initialize()
        {
            AddRequiredType<Items.Diary>();
        }
        public sealed class DiaryAchievement : BaseAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                GlobalEventManager.onCharacterLevelUp += CheckTeamLevel;
            }

            public override void OnUninstall()
            {
                GlobalEventManager.onCharacterLevelUp -= CheckTeamLevel;
                base.OnUninstall();
            }

            private void CheckTeamLevel(CharacterBody characterBody)
            {
                uint teamLevel = TeamManager.instance.GetTeamLevel(characterBody.teamComponent.teamIndex);

                if (characterBody.teamComponent.teamIndex == TeamIndex.Player)
                {
                    if (teamLevel >= 20u && characterBody.isPlayerControlled)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}