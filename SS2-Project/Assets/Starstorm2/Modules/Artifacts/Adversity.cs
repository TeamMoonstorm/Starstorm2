using MSU;
using R2API.ScriptableObjects;
using RoR2;
using System.Collections;
using UnityEngine.SceneManagement;
using RoR2.ContentManagement;
using UnityEngine.Networking;
namespace SS2.Artifacts
{
#if DEBUG
    public class Adversity : SS2Artifact
    {
        public override SS2AssetRequest assetRequest => SS2Assets.LoadAssetAsync<ArtifactAssetCollection>("acAdversity", SS2Bundle.Artifacts);

        public static bool shouldUpgradeTP;
        public static float timer;
        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public override void OnArtifactDisabled()
        {
            On.RoR2.SceneDirector.Start -= SceneDirector_Start;
            shouldUpgradeTP = false;
            //Ethereal.adversityEnabled = false;
        }

        public override void OnArtifactEnabled()
        {
            On.RoR2.SceneDirector.Start += SceneDirector_Start;
            shouldUpgradeTP = false;
            //Ethereal.adversityEnabled = true;
        }

        public static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            orig(self);
            if (!NetworkServer.active) return;

            var currStage = SceneManager.GetActiveScene().name;
            if (self.teleporterInstance && (currStage == "skymeadow" || currStage == "slumberingsatellite"))
            {
                if (TeleporterUpgradeController.instance)
                    TeleporterUpgradeController.instance.UpgradeEthereal();
            }
        }
    }
#endif
}
