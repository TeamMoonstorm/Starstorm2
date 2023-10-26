using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2
{
    class Bulwark
    {
        public static bool swordBulwark = false; //always true for now for testing purposes.

        internal static void Init()
        {
            //Hooks
            //On.RoR2.SceneDirector.Start += SceneDirector_Start;
            On.RoR2.ArtifactTrialMissionController.OnCurrentArtifactDiscovered += OverwriteArtifact;
        }

        private static void SceneDirector_Start(On.RoR2.SceneDirector.orig_Start orig, SceneDirector self)
        {
            //nothing now; maybe later?
            if (SceneManager.GetActiveScene().name == "artifactworld" && swordBulwark && NetworkServer.active)
            {
                Debug.Log("cool sword!");
            }
        }

        private static void OverwriteArtifact(On.RoR2.ArtifactTrialMissionController.orig_OnCurrentArtifactDiscovered orig, ArtifactTrialMissionController self, ArtifactDef artifactDef)
        {
            orig(self, artifactDef);
            if (self.artifactPickup && swordBulwark)
            {
                //unless the artifact was already active in the run, disable it from the ambry
                if (!self.artifactWasEnabled)
                {
                    RunArtifactManager.instance.SetArtifactEnabledServer(artifactDef, false);
                }
                //replace the current artifact pickup with the blessed blade of the bulwark
                self.artifactPickup.NetworkpickupIndex = PickupCatalog.FindPickupIndex(SS2Content.Equipments.equipDivineRight.equipmentIndex);
            }
        }
    }
}
