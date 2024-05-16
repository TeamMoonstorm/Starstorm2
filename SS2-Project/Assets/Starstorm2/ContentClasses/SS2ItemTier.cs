using MSU;
using R2API.ScriptableObjects;
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
    /// <inheritdoc cref="IItemTierContentPiece"/>
    /// </summary>
    public abstract class SS2ItemTier : IItemTierContentPiece, IContentPackModifier
    {
        public ItemTierAssetCollection AssetCollection { get; private set; }
        public NullableRef<SerializableColorCatalogEntry> ColorIndex { get; protected set; }
        public NullableRef<SerializableColorCatalogEntry> DarkColorIndex { get; protected set; }
        public GameObject PickupDisplayVFX { get; protected set; }
        public List<ItemIndex> ItemsWithThisTier { get; set; } = new List<ItemIndex>();
        public List<PickupIndex> AvailableTierDropList { get; set; } = new List<PickupIndex>();
        ItemTierDef IContentPiece<ItemTierDef>.Asset => ItemTierDef;
        public ItemTierDef ItemTierDef { get; protected set;  }

        public abstract SS2AssetRequest<ItemTierAssetCollection> AssetRequest { get; }
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<ItemTierAssetCollection> request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;
            ItemTierDef = AssetCollection.itemTierDef;
            
            if (AssetCollection.colorIndex)
                ColorIndex = AssetCollection.colorIndex;
            if (AssetCollection.darkColorIndex)
                DarkColorIndex = AssetCollection.darkColorIndex;

            PickupDisplayVFX = AssetCollection.pickupDisplayVFX;
        }

        public void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}