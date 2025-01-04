using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
    public sealed class HorseshoeAchievement : BaseAchievement
    {
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            TeleporterInteraction.onTeleporterChargedGlobal += CheckTeleporterCompletion;
        }

        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            TeleporterInteraction.onTeleporterChargedGlobal -= CheckTeleporterCompletion;
        }

        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("Bandit2Body");
        }
        private void CheckTeleporterCompletion(TeleporterInteraction teleporterInteraction)
        {
            if (Run.instance.stageClearCount == 2 && Run.instance.GetRunStopwatch() < timeRequirement && DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty) == Typhoon.sdd.DifficultyDef)
            {
                Grant();
            }
        }
        static float timeRequirement = 900f; //15 miunutese
    }
}