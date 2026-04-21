using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace SS2
{
    public static class GameModeModule
    {
        internal static IEnumerator Init()
        {
            var classicRunPrefab = Addressables.LoadAssetAsync<GameObject>(
                "RoR2/Base/ClassicRun/ClassicRun.prefab").WaitForCompletion();
            var blitzRunPrefab = classicRunPrefab.InstantiateClone("BlitzRun");

            if (!blitzRunPrefab.TryGetComponent<Run>(out var run))
            {
                SS2Log.Error("GameModeModule: Cloned ClassicRun has no Run component.");
                yield break;
            }

            run.nameToken = "SS2_GAMEMODE_BLITZ_NAME";
            run.userPickable = true;

            blitzRunPrefab.AddComponent<BlitzRunBehavior>();
            blitzRunPrefab.AddComponent<GameModeInfo>().buttonHoverDescription = "SS2_GAMEMODE_BLITZ_DESC";

            SS2Content.SS2ContentPack.gameModePrefabs.Add(new[] { blitzRunPrefab });

            SS2Log.Info("Blitz gamemode registered.");
            yield break;
        }
    }
}
