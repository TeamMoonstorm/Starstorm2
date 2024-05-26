using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using R2API;
using UnityEngine.AddressableAssets;
using MSU;
using System.Collections;
using RoR2.ContentManagement;

namespace SS2.Scenes
{
    public sealed class VoidShop : SS2Scene
    {
        public static GameObject portalPrefab;

        public override SS2AssetRequest<SceneAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SceneAssetCollection>("acVoidShop", SS2Bundle.SharedStages);

        public override void Initialize()
        {
            portalPrefab = PrefabAPI.InstantiateClone(SS2Assets.LoadAsset<GameObject>("PortalStrangerExit", SS2Bundle.SharedStages), "StrangerPortal", true);
            portalPrefab.RegisterNetworkPrefab();
        }


        public override void OnServerStageBegin(Stage stage)
        {
            //summon trader
            var traderSummon = new MasterSummon();
            traderSummon.position = new Vector3(-26f, 0f, 65.5f);
            traderSummon.rotation = new Quaternion(0f, 90f, 0f, 0f);
            traderSummon.masterPrefab = SS2Assets.LoadAsset<GameObject>("TraderMaster", SS2Bundle.Indev);
            traderSummon.teamIndexOverride = TeamIndex.Neutral;
            traderSummon.Perform();

            //place exit portal
            var position = new Vector3(9f, 35f, -110f);
            var rotation = Quaternion.Euler(330, 0, 0);
            GameObject portal = GameObject.Instantiate(SS2Assets.LoadAsset<GameObject>("PortalStrangerExit", SS2Bundle.SharedStages), position, rotation);
            NetworkServer.Spawn(portal);
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
