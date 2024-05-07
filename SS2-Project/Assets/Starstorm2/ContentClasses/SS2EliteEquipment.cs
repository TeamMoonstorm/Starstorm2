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
    /// <inheritdoc cref="IEliteContentPiece"/>
    /// </summary>
    public abstract class SS2EliteEquipment : IEliteContentPiece
    {
        public List<EliteDef> EliteDefs;
        List<EliteDef> IEliteContentPiece.EliteDefs => EliteDefs;
        public EliteAssetCollection AssetCollection { get; private set; }

        public NullableRef<List<GameObject>> ItemDisplayPrefabs;
        NullableRef<List<GameObject>> IEquipmentContentPiece.ItemDisplayPrefabs => ItemDisplayPrefabs;
        EquipmentDef IContentPiece<EquipmentDef>.Asset => EquipmentDef;     

        public EquipmentDef EquipmentDef;
        public abstract SS2AssetRequest<EliteAssetCollection> AssetRequest();
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<EliteAssetCollection> request = AssetRequest();

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            EliteDefs = AssetCollection.eliteDefs;
            EquipmentDef = AssetCollection.equipmentDef;
            ItemDisplayPrefabs = AssetCollection.itemDisplayPrefabs;

            OnAssetCollectionLoaded(AssetCollection);
        }

        public virtual void OnAssetCollectionLoaded(AssetCollection assetCollection) { }

        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public abstract bool Execute(EquipmentSlot slot);
        public abstract void OnEquipmentLost(CharacterBody body);
        public abstract void OnEquipmentObtained(CharacterBody body);
    }
}