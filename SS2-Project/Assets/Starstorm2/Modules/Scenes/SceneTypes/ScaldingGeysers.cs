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
    public sealed class ScaldingGeysers : SS2Scene
    {
        public override SS2AssetRequest<SceneAssetCollection> assetRequest => SS2Assets.LoadAssetAsync<SceneAssetCollection>("acScaldingGeysers", SS2Bundle.SharedStages);

        public override void Initialize()
        {
            sceneDef.mainTrack = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/MusicTrackDefs/muGameplayBase_09.asset").WaitForCompletion();

            sceneDef.bossTrack = Addressables.LoadAssetAsync<MusicTrackDef>("RoR2/Base/Common/MusicTrackDefs/muSong16.asset").WaitForCompletion();
        }



        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
