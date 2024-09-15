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
        public override SS2AssetRequest<SceneAssetCollection> AssetRequest => SS2Assets.LoadAssetAsync<SceneAssetCollection>("acVoidShop", SS2Bundle.SharedStages);

        public override void Initialize()
        {
            Asset.mainTrack = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/muSong08.asset").WaitForCompletion();
        }


        public override void OnServerStageBegin(Stage stage)
        {
            //summon trader
            var traderSummon = new MasterSummon();
            traderSummon.position = new Vector3(-0.88f, 0.47f, 60.15f);
            traderSummon.rotation = new Quaternion(0f, 90f, 0f, 0f);
            traderSummon.masterPrefab = SS2Assets.LoadAsset<GameObject>("TraderMaster", SS2Bundle.Indev);
            traderSummon.teamIndexOverride = TeamIndex.Neutral;
            traderSummon.Perform();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }
    }
}
