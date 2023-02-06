using RoR2;
using RoR2.Achievements;

namespace Moonstorm.Starstorm2.Unlocks.Pickups
{
    public sealed class MaliceUnlockable : UnlockableBase
    {
        public override MSUnlockableDef UnlockableDef { get; } = SS2Assets.LoadAsset<MSUnlockableDef>("ss2.item.malice", SS2Bundle.Items);

        public override void Initialize()
        {
            AddRequiredType<Items.Malice>();
        }
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
                        if (currentRun.selectedDifficulty == Typhoon.TyphoonIndex && sceneDefForCurrentScene.stageOrder == 3)
                        {
                            Grant();
                        }
                    }
                }
            }
        }
    }
}