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

        static Material FUCK;
        public override void Initialize()
        {
            Stage.onServerStageBegin += SpawnTable;

            FUCK = SS2Assets.LoadAsset<Material>("tmpBombDropshadowHologramCursed", SS2Bundle.Base);
            FUCK.shader = LegacyShaderAPI.Find("TextMeshPro/Distance Field");
        }

        private void SpawnTable(Stage stage)
        {
            if(DirectorAPI.GetStageEnumFromSceneDef(stage.sceneDef) == DirectorAPI.Stage.Bazaar)
            {
                // idk
                

                GameObject table = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("LunarGambler", SS2Bundle.Interactables), position, Quaternion.Euler(rotation));
                NetworkServer.Spawn(table);
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return SS2Config.enableBeta;
        }

    }
}
