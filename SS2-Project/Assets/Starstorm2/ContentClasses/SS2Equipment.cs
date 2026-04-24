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
using RiskOfOptions.OptionConfigs;
using MSU.Config;

namespace SS2
{
    /// <summary>
    /// <inheritdoc cref="IEquipmentContentPiece"/>
    /// </summary>
    public abstract class SS2Equipment : IEquipmentContentPiece, IContentPackModifier, IAsyncContentInitializer
    {
        public EquipmentAssetCollection AssetCollection { get; private set; }

        public NullableRef<List<GameObject>> ItemDisplayPrefabs { get; protected set; } = new List<GameObject>();
        NullableRef<List<GameObject>> IEquipmentContentPiece.itemDisplayPrefabs => ItemDisplayPrefabs;
        EquipmentDef IContentPiece<EquipmentDef>.asset => EquipmentDef;

        public EquipmentDef EquipmentDef { get; protected set; }

        public abstract SS2AssetRequest AssetRequest { get; }
        public abstract void Initialize();
        public virtual IEnumerator InitializeAsync()
        {
            if(AssetCollection && AssetCollection.itemDisplayAddressedDictionary.TryGetValue(out var idad))
            {
                var subroutine = idad.AddEntries();
                while(subroutine.MoveNext())
                {
                    yield return null;
                }
            }
        }
        public virtual bool IsAvailable(ContentPack conentPack)
        {
            ConfiguredBool isDisabled = SS2Config.ConfigFactory.MakeConfiguredBool(false, b =>
            {
                b.section = "00 - Equipment Disabling";
                b.key = $"Disable Equipment: {MSUtil.NicifyString(GetType().Name)}";
                b.description = "Set this to true if you want to disable this equipment from appearing in game. Make sure everyone has this enabled or disabled in multiplayer otherwise desyncs could occur as this prevents the content from being loaded entirely. Restart required.";
                b.configFile = SS2Config.ConfigItem;
                b.checkBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true
                };
            }).DoConfigure();

            return !isDisabled;
        }

        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            if (request.BoxedAsset is EquipmentAssetCollection collection)
            {
                AssetCollection = collection;

                EquipmentDef = AssetCollection.equipmentDef;
                ItemDisplayPrefabs = AssetCollection.itemDisplayPrefabs;
            }
            else if (request.BoxedAsset is EquipmentDef def)
            {
                EquipmentDef = def;
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

        public abstract bool Execute(EquipmentSlot slot);       
        public abstract void OnEquipmentLost(CharacterBody body);
        public abstract void OnEquipmentObtained(CharacterBody body);
    }
}