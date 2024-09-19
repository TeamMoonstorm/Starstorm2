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
        public NullableRef<List<GameObject>> ItemDisplayPrefabs { get; protected set; } = new List<GameObject>();
        public ItemDef ItemDef { get; protected set; }

        ItemDef IContentPiece<ItemDef>.asset => ItemDef;
        NullableRef<List<GameObject>> IItemContentPiece.itemDisplayPrefabs => ItemDisplayPrefabs;

        public abstract SS2AssetRequest AssetRequest { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            if(request.BoxedAsset is ItemAssetCollection collection)
            {
                AssetCollection = collection;

                ItemDef = AssetCollection.itemDef;
                ItemDisplayPrefabs = AssetCollection.itemDisplayPrefabs;
            }
            else if(request.BoxedAsset is ItemDef def)
            {
                ItemDef = def;
            }
            else
            {
                SS2Log.Error("Invalid AssetRequest " + request.AssetName + " of type " + request.BoxedAsset.GetType());
            }
        }

        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            if(AssetCollection)
                contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}