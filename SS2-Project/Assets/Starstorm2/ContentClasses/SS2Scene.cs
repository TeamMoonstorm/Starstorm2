using MSU;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2
{
    public abstract class SS2Scene : ISceneContentPiece, IContentPackModifier
    {
        public SceneAssetCollection AssetCollection { get; private set; }
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public abstract SS2AssetRequest<SceneAssetCollection> AssetRequest { get; }

        public NullableRef<MusicTrackDef> MainTrack => Asset.mainTrack;
        public NullableRef<MusicTrackDef> BossTrack => Asset.bossTrack;

        public Texture2D BazaarTextureBase { get; protected set; } // ???

        public SceneDef Asset { get; protected set; }

        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<SceneAssetCollection> request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            Asset = AssetCollection.sceneDef;

        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public virtual void OnServerStageComplete(Stage stage)
        {
        }

        public virtual void OnServerStageBegin(Stage stage)
        {           
        }
    }
}
