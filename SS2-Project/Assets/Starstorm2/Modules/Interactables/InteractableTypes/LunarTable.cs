using RoR2;
using RoR2.ContentManagement;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Interactables
{
    public sealed class LunarTable : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acLunarTable", SS2Bundle.Interactables);

        public static Vector3 position;
        public static Vector3 rotation;
        public override void Initialize()
        {
            Stage.onServerStageBegin += SpawnTable;
        }

        private void SpawnTable(Stage stage)
        {
            if(DirectorAPI.GetStageEnumFromSceneDef(stage.sceneDef) == DirectorAPI.Stage.Bazaar)
            {
                GameObject table = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("LunarTable", SS2Bundle.Interactables), position, Quaternion.Euler(rotation));
                NetworkServer.Spawn(table);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }
}
