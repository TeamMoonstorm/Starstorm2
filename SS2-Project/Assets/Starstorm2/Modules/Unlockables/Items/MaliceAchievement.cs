﻿using RoR2;
using RoR2.Achievements;
namespace SS2.Unlocks.Pickups
{
    public sealed class MaliceAchievement : BaseAchievement
    {
        public override void OnInstall()
        {
            base.OnInstall();
            TeleporterInteraction.onTeleporterChargedGlobal += CheckTeleporterCompletion;
        }

        public override void OnUninstall()
        {
            TeleporterInteraction.onTeleporterChargedGlobal -= CheckTeleporterCompletion;
            base.OnUninstall();
        }
        private void CheckTeleporterCompletion(TeleporterInteraction teleporterInteraction)
        {
            if ((bool)Run.instance && Run.instance.GetType() == typeof(Run))
            {
                SceneDef sceneDefForCurrentScene = SceneCatalog.GetSceneDefForCurrentScene();
                Run currentRun = Run.instance;
                if (!(sceneDefForCurrentScene == null))
                {
                    /*TO-DO:
                     * We are probably going to need to catalog DifficultyDefs in SS2Content because checks for SS2
                     * difficulty indeces will not work as we would need a reference to an instance of the Def. Let me
                     * know if there is a solution other than storing the difficulties.
                     * - J
                    */

                    if (/*currentRun.selectedDifficulty == Typhoon.DifficultyDef.DifficultyIndex &&*/ sceneDefForCurrentScene.stageOrder == 3)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}