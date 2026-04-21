using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SS2
{
    public static class GameModeModule
    {
        private static GameObject blitzRunPrefab;

        internal static IEnumerator Init()
        {
            // Inactive holder prevents Awake/OnEnable on the clone
            var holder = new GameObject("SS2_GameModePrefabs");
            holder.SetActive(false);
            Object.DontDestroyOnLoad(holder);

            // Clone ClassicRun under inactive parent
            var classicRunPrefab = Addressables.LoadAssetAsync<GameObject>(
                "RoR2/Base/ClassicRun/ClassicRun.prefab").WaitForCompletion();
            blitzRunPrefab = Object.Instantiate(classicRunPrefab, holder.transform);
            blitzRunPrefab.name = "BlitzRun";

            // Swap Run to BlitzRun, preserving all fields
            var oldRun = blitzRunPrefab.GetComponent<Run>();
            if (oldRun == null)
            {
                SS2Log.Error("GameModeModule: Cloned prefab has no Run component.");
                yield break;
            }

            var saved = new RunFieldSnapshot(oldRun);
            Object.DestroyImmediate(oldRun);

            var blitzRun = blitzRunPrefab.AddComponent<BlitzRun>();
            if (blitzRun == null)
            {
                SS2Log.Error("GameModeModule: Failed to add BlitzRun component.");
                yield break;
            }

            saved.ApplyTo(blitzRun);
            blitzRun.nameToken = "SS2_GAMEMODE_BLITZ_NAME";
            blitzRun.userPickable = true;

            // Remove ClassicRun-only components we don't need
            if (blitzRunPrefab.TryGetComponent<TutorialManager>(out var tutorial))
                Object.DestroyImmediate(tutorial);
            if (blitzRunPrefab.TryGetComponent<PreloadContent>(out var preload))
                Object.DestroyImmediate(preload);

            // Register for networking AFTER component swap
            PrefabAPI.RegisterNetworkPrefab(blitzRunPrefab);

            // R2API hooks
            blitzRunPrefab.AddComponent<GameModeInfo>().buttonHoverDescription = "SS2_GAMEMODE_BLITZ_DESC";

            SS2Content.SS2ContentPack.gameModePrefabs.Add(new[] { blitzRunPrefab });

            // Populate startingScenes with all valid stages after catalogs init
            RoR2Application.onLoad += PopulateStartingScenes;

            SS2Log.Info("Blitz gamemode registered.");
            yield break;
        }

        private static void PopulateStartingScenes()
        {
            if (blitzRunPrefab == null || !blitzRunPrefab.TryGetComponent<BlitzRun>(out var blitzRun))
            {
                SS2Log.Error("GameModeModule: BlitzRun prefab or component missing.");
                return;
            }

            blitzRun.startingScenes = SceneCatalog.allStageSceneDefs
                .Where(s => s.validForRandomSelection)
                .ToArray();

            SS2Log.Info("Blitz gamemode: populated " + blitzRun.startingScenes.Length + " starting scenes.");
        }

        /// <summary>
        /// Saves serialized Run fields so they survive a component swap.
        /// </summary>
        private struct RunFieldSnapshot
        {
            public SceneCollection startingSceneGroup;
            public SceneDef[] startingScenes;
            public GameObject gameOverPrefab;
            public GameObject lobbyBackgroundPrefab;
            public GameObject uiPrefab;
            public PickupDropTable rebirthDropTable;

            public RunFieldSnapshot(Run run)
            {
                startingSceneGroup = run.startingSceneGroup;
                startingScenes = run.startingScenes;
                gameOverPrefab = run.gameOverPrefab;
                lobbyBackgroundPrefab = run.lobbyBackgroundPrefab;
                uiPrefab = run.uiPrefab;
                rebirthDropTable = run.rebirthDropTable;
            }

            public void ApplyTo(Run run)
            {
                run.startingSceneGroup = startingSceneGroup;
                run.startingScenes = startingScenes;
                run.gameOverPrefab = gameOverPrefab;
                run.lobbyBackgroundPrefab = lobbyBackgroundPrefab;
                run.uiPrefab = uiPrefab;
                run.rebirthDropTable = rebirthDropTable;
            }
        }
    }
}
