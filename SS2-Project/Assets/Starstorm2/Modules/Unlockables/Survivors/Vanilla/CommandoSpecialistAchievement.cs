using RoR2;
using RoR2.Achievements;
using UnityEngine;

namespace SS2.Unlocks.VanillaSurvivors
{
    public sealed class CommandoSpecialistAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex()
        {
            return BodyCatalog.FindBodyIndex("CommandoBody");
        }
        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            SetServerTracked(true);
        }
        public override void OnBodyRequirementBroken()
        {
            base.OnBodyRequirementBroken();
            SetServerTracked(false);
        }
        private class CommandoSpecialistServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                Stage.onServerStageBegin += Stage4Fast;
            }

            private void Stage4Fast(Stage stage)
            {
                if (Run.instance.stageClearCount == 3 && Run.instance.time < (15 * 60))
                {
                    Grant();
                }
            }

            public override void OnUninstall()
            {
                Stage.onServerStageBegin -= Stage4Fast;
                base.OnUninstall();
            }
        }
    }
    
}