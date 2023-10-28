using RoR2;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Moonstorm.Starstorm2.Artifacts
{
    [DisabledContent]
    public class Adversity : ArtifactBase
    {
        public override ArtifactDef ArtifactDef { get; } = SS2Assets.LoadAsset<ArtifactDef>("Adversity", SS2Bundle.Artifacts);

        private static bool shouldUpgradeTP;
        private static float timer;

        public override void OnArtifactDisabled()
        {
            On.RoR2.SceneDirector.Start -= SceneDirector_Start;
            shouldUpgradeTP = false;
            Ethereal.adversityEnabled = false;
        }

        public override void OnArtifactEnabled()
        {
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            shouldUpgradeTP = false;
            Ethereal.adversityEnabled = true;
        }

        public static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);

            var currStage = SceneManager.GetActiveScene().name;
            if (self.teleporterInstance && (currStage == "skymeadow" || currStage == "slumberingsatellite"))
            {
                TeleporterUpgradeController tuc = self.teleporterInstance.GetComponent<TeleporterUpgradeController>();
                if (tuc != null)
                    tuc.UpgradeTeleporter();
            }
        }
    }
}
