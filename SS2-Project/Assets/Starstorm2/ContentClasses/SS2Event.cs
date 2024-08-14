using MSU;
using RoR2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ContentManagement;
namespace SS2
{
    /// <summary>
    /// <inheritdoc cref="IGameObjectContentPiece"/>
    /// </summary>
    public abstract class SS2Event : IGameObjectContentPiece<GameplayEvent>, IContentPackModifier
    {
        public AssetCollection AssetCollection { get; private set; }
        GameplayEvent IGameObjectContentPiece<GameplayEvent>.Component => EventPrefab.GetComponent<GameplayEvent>();
        GameObject IContentPiece<GameObject>.Asset => EventPrefab;
        public GameObject EventPrefab { get; protected set; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public abstract SS2AssetRequest AssetRequest { get; }

        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            if (request.BoxedAsset is ExtendedAssetCollection assetCollection)
            {
                AssetCollection = assetCollection;
                EventPrefab = assetCollection.assets.Where(o => o is GameObject).Select(go => (go as GameObject).GetComponent<GameplayEvent>().gameObject).FirstOrDefault();
            }
            else if (request.BoxedAsset is GameObject gameObject && gameObject.GetComponent<GameplayEvent>())
            {
                EventPrefab = gameObject;
            }
            else
            {
                SS2Log.Error("Invalid AssetRequest " + request.AssetName + " of type " + request.BoxedAsset.GetType());
            }

        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            GameplayEventCatalog.AddGameplayEvent(EventPrefab);
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}