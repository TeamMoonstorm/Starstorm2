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
using MSU.Config;
using RiskOfOptions.OptionConfigs;

namespace SS2
{
    /// <summary>
    /// <inheritdoc cref="IVoidItemContentPiece"/>
    /// </summary>
    public abstract class SS2VoidItem : IVoidItemContentPiece, IContentPackModifier
    {
        public ItemAssetCollection AssetCollection { get; private set; }
        public NullableRef<List<GameObject>> ItemDisplayPrefabs { get; protected set; } = new List<GameObject>();
        public ItemDef ItemDef { get; protected set; }

        ItemDef IContentPiece<ItemDef>.asset => ItemDef;
        NullableRef<List<GameObject>> IItemContentPiece.itemDisplayPrefabs => ItemDisplayPrefabs;

        public abstract SS2AssetRequest AssetRequest();

        public abstract void Initialize();

        public static ConfiguredBool DisableItem = SS2Config.ConfigFactory.MakeConfiguredBool(false, b =>
        {
            b.section = SS2Config.ID_ITEM;
            b.key = "Disable item?";
            b.description = "Set this to true if you want to disable this item from appearing in game.";
            b.configFile = SS2Config.ConfigEvent;
            b.checkBoxConfig = new CheckBoxConfig
            {
                restartRequired = true
            };
        }).DoConfigure();

        public virtual bool IsAvailable(ContentPack contentPack)
        {
            return !DisableItem;
        }
        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest request = AssetRequest();

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            if (request.BoxedAsset is ItemAssetCollection collection)
            {
                AssetCollection = collection;

                ItemDef = AssetCollection.itemDef;
                ItemDisplayPrefabs = AssetCollection.itemDisplayPrefabs;
            }
            else if (request.BoxedAsset is ItemDef def)
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
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }

        public abstract List<ItemDef> GetInfectableItems();
    }
}