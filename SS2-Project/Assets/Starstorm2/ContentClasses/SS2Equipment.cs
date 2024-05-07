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
    /// <inheritdoc cref="IEquipmentContentPiece"/>
    /// </summary>
    public abstract class SS2Equipment : IEquipmentContentPiece
    {
        public EquipmentAssetCollection AssetCollection { get; private set; }

        public NullableRef<List<GameObject>> ItemDisplayPrefabs;
        NullableRef<List<GameObject>> IEquipmentContentPiece.ItemDisplayPrefabs => ItemDisplayPrefabs;
        EquipmentDef IContentPiece<EquipmentDef>.Asset => EquipmentDef;

        public EquipmentDef EquipmentDef;

        public abstract SS2AssetRequest<T> AssetRequest<T>() where T : UnityEngine.Object;
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<UnityEngine.Object> request = AssetRequest<UnityEngine.Object>();

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            if (request.Asset is EquipmentAssetCollection)
            {
                AssetCollection = (EquipmentAssetCollection)request.Asset;

                EquipmentDef = AssetCollection.equipmentDef;
                ItemDisplayPrefabs = AssetCollection.itemDisplayPrefabs;

                OnAssetCollectionLoaded(AssetCollection);
            }
            else if (request.Asset is EquipmentDef)
            {
                EquipmentDef = (EquipmentDef)request.Asset;
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

        public abstract bool Execute(EquipmentSlot slot);       
        public abstract void OnEquipmentLost(CharacterBody body);
        public abstract void OnEquipmentObtained(CharacterBody body);
    }
}