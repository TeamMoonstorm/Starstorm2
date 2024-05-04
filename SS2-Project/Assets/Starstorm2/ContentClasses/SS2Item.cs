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
    /// <inheritdoc cref="IItemContentPiece"/>
    /// </summary>
    public abstract class SS2Item : IItemContentPiece, IContentPackModifier
    {
        public ItemAssetCollection AssetCollection { get; private set; }
        public NullableRef<List<GameObject>> ItemDisplayPrefabs;
        public ItemDef ItemDef;

        ItemDef IContentPiece<ItemDef>.Asset => ItemDef;
        NullableRef<List<GameObject>> IItemContentPiece.ItemDisplayPrefabs => ItemDisplayPrefabs;

        public abstract SS2AssetRequest<T> AssetRequest<T>() where T : UnityEngine.Object;

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<UnityEngine.Object> request = AssetRequest<UnityEngine.Object>();

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            if((ItemAssetCollection)request.Asset)
            {
                AssetCollection = (ItemAssetCollection)request.Asset;

                ItemDef = AssetCollection.itemDef;
                ItemDisplayPrefabs = AssetCollection.itemDisplayPrefabs;

                OnAssetCollectionLoaded(AssetCollection);
            }
            else if((ItemDef)request.Asset)
            {
                ItemDef = (ItemDef)request.Asset;
            }
            else
            {
                SS2Log.Error("Invalid AssetRequest " + request.AssetName + " of type " + request.Asset.GetType());
            }
        }

        public virtual void OnAssetCollectionLoaded(AssetCollection assetCollection) { }

        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}