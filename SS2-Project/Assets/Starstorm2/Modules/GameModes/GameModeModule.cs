using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace SS2
{
    public static class GameModeModule
    {
        private static GameObject blitzRunPrefab;

        internal static IEnumerator Init()
        {
            var holder = new GameObject("SS2_GameModePrefabs");
            holder.SetActive(false);
            Object.DontDestroyOnLoad(holder);

            blitzRunPrefab = new GameObject("BlitzRun");
            blitzRunPrefab.transform.SetParent(holder.transform);

            // [RequireComponent] on Run auto-adds NetworkIdentity, NetworkRuleBook, RunArtifactManager
            var blitzRun = blitzRunPrefab.AddComponent<BlitzRun>();
            blitzRun.nameToken = "SS2_GAMEMODE_BLITZ_NAME";
            blitzRun.userPickable = true;

            // Register for networking so NetworkServer.Spawn works
            PrefabAPI.RegisterNetworkPrefab(blitzRunPrefab);

            // Add GameModeInfo for R2API's GameModeFixes UI hooks
            blitzRunPrefab.AddComponent<GameModeInfo>().buttonHoverDescription = "SS2_GAMEMODE_BLITZ_DESC";

            // Add directly to SS2's content pack 
            SS2Content.SS2ContentPack.gameModePrefabs.Add(new[] { blitzRunPrefab });

            // Copy startingSceneGroup/gameOverPrefab/lobbyBackgroundPrefab from ClassicRun after catalogs init
            RoR2Application.onLoad += CopyClassicRunFields;

            SS2Log.Info("Blitz gamemode registered.");
            yield break;
        }

        // TODO: Temp
        private static void CopyClassicRunFields()
        {
            var classicIndex = GameModeCatalog.FindGameModeIndex("ClassicRun");
            if (classicIndex == GameModeIndex.Invalid)
            {
                SS2Log.Error("GameModeModule: Could not find ClassicRun to copy fields from.");
                return;
            }

            var classicRun = GameModeCatalog.GetGameModePrefabComponent(classicIndex);
            if (classicRun == null)
            {
                SS2Log.Error("GameModeModule: ClassicRun prefab component is null.");
                return;
            }

            if (blitzRunPrefab == null || !blitzRunPrefab.TryGetComponent<BlitzRun>(out var blitzRun))
            {
                SS2Log.Error("GameModeModule: BlitzRun prefab or component missing.");
                return;
            }

            blitzRun.startingSceneGroup = classicRun.startingSceneGroup;
            blitzRun.gameOverPrefab = classicRun.gameOverPrefab;
            blitzRun.lobbyBackgroundPrefab = classicRun.lobbyBackgroundPrefab;

            blitzRun.startingScenes = SceneCatalog.allStageSceneDefs
                .Where(s => s.validForRandomSelection)
                .ToArray();

            SS2Log.Info("Blitz gamemode: copied fields from ClassicRun. startingScenes: " + blitzRun.startingScenes.Length + " scenes.");
        }
    }
}
