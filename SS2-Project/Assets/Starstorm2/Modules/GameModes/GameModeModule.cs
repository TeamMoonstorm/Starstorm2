using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.ExpansionManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace SS2
{
    public static class GameModeModule
    {
        private static GameObject blitzRunPrefab;

        internal static IEnumerator Init()
        {
        
            blitzRunPrefab = PrefabAPI.InstantiateClone(new GameObject("BlitzRun"), "BlitzRun");
            blitzRunPrefab.AddComponent<NetworkIdentity>();
            PrefabAPI.RegisterNetworkPrefab(blitzRunPrefab);

            // Load ClassicRun to copy field values
            var classicRunPrefab = Addressables.LoadAssetAsync<GameObject>(
                "RoR2/Base/ClassicRun/ClassicRun.prefab").WaitForCompletion();
            var classicRun = classicRunPrefab.GetComponent<Run>();

            // Add BlitzRun and copy ClassicRun's configuration
            var blitzRun = blitzRunPrefab.AddComponent<BlitzRun>();
            blitzRun.nameToken = "SS2_GAMEMODE_BLITZ_NAME";
            blitzRun.userPickable = true;
            blitzRun.startingSceneGroup = classicRun.startingSceneGroup;
            blitzRun.startingScenes = classicRun.startingScenes;
            blitzRun.gameOverPrefab = classicRun.gameOverPrefab;
            blitzRun.lobbyBackgroundPrefab = classicRun.lobbyBackgroundPrefab;
            blitzRun.uiPrefab = classicRun.uiPrefab;
            blitzRun.rebirthDropTable = classicRun.rebirthDropTable;

            blitzRunPrefab.AddComponent<TeamManager>();
            blitzRunPrefab.AddComponent<NetworkRuleBook>();
            blitzRunPrefab.AddComponent<TeamFilter>();
            blitzRunPrefab.AddComponent<EnemyInfoPanelInventoryProvider>();
            blitzRunPrefab.AddComponent<DirectorCore>();
            //blitzRunPrefab.AddComponent<ExpansionRequirementComponent>();
            blitzRunPrefab.AddComponent<RunCameraManager>();

            blitzRunPrefab.AddComponent<GameModeInfo>().buttonHoverDescription = "SS2_GAMEMODE_BLITZ_DESC";

            SS2Content.SS2ContentPack.gameModePrefabs.Add(new[] { blitzRunPrefab });

            SS2Log.Info("Blitz gamemode registered.");
            yield break;
        }
    }
}
