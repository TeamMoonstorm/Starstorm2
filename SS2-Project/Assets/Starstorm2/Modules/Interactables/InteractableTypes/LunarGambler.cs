using RoR2;
using RoR2.ContentManagement;
using R2API;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Interactables
{
    public sealed class LunarGambler : SS2Interactable
    {
        public override SS2AssetRequest<InteractableAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<InteractableAssetCollection>("acLunarGambler", SS2Bundle.Interactables);

        public static Vector3 position = new Vector3(-101f, -25.4f, -49.3f); // placement sucks. temp
        public static Vector3 rotation = new Vector3 (0, -90f, 0);
        public override void Initialize()
        {
            Stage.onServerStageBegin += SpawnTable;
        }

        private void SpawnTable(Stage stage)
        {
            if(DirectorAPI.GetStageEnumFromSceneDef(stage.sceneDef) == DirectorAPI.Stage.Bazaar)
            {
                GameObject table = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("LunarGambler", SS2Bundle.Interactables), position, Quaternion.Euler(rotation));
                NetworkServer.Spawn(table);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }
}
