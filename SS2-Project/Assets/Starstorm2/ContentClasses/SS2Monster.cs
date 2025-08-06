using MSU;
using R2API;
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
    /// <inheritdoc cref="IMonsterContentPiece"/>
    /// </summary>
    public abstract class SS2Monster : IMonsterContentPiece, IContentPackModifier
    {
        public NullableRef<MonsterCardProvider> CardProvider { get; protected set; }
        public NullableRef<DirectorCardHolderExtended> DissonanceCard { get; protected set; }
        public MonsterAssetCollection AssetCollection { get; private set; }
        public NullableRef<GameObject> masterPrefab { get; protected set; }

        NullableRef<DirectorCardHolderExtended> IMonsterContentPiece.dissonanceCard => DissonanceCard;
        CharacterBody IGameObjectContentPiece<CharacterBody>.component => CharacterPrefab.GetComponent<CharacterBody>();
        NullableRef<MonsterCardProvider> IMonsterContentPiece.cardProvider => CardProvider;
        GameObject IContentPiece<GameObject>.asset => CharacterPrefab;
        public GameObject CharacterPrefab { get; private set; }

        public abstract void Initialize();
        public virtual bool IsAvailable(ContentPack conentPack)
        {
            ConfiguredBool isDisabled = SS2Config.ConfigFactory.MakeConfiguredBool(false, b =>
            {
                b.section = "00 - Enemy Disabling";
                b.key = $"Disable Enemy: {MSUtil.NicifyString(GetType().Name)}";
                b.description = "Set this to true if you want to disable this enemy from appearing in game.";
                b.configFile = SS2Config.ConfigMonster;
                b.checkBoxConfig = new CheckBoxConfig
                {
                    restartRequired = true
                };
            }).DoConfigure();

            return !isDisabled;
        }

        public abstract SS2AssetRequest<MonsterAssetCollection> AssetRequest { get; }

        

        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<MonsterAssetCollection> request = AssetRequest;

            if (request == null)
            {
                SS2Log.Warning($"AssetRequest is null for {GetType().Name}");
                yield break;
            }

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            if (AssetCollection == null)
            {
                SS2Log.Warning($"AssetCollection is null for {GetType().Name}");
                yield break;
            }

            if (AssetCollection.bodyPrefab != null)
            {
                CharacterPrefab = AssetCollection.bodyPrefab;
            }
            else
            {
                SS2Log.Warning($"bodyPrefab is null in AssetCollection for {GetType().Name}");
            }

            if (AssetCollection.masterPrefab != null)
            {
                masterPrefab = AssetCollection.masterPrefab;
            }
            else
            {
                SS2Log.Warning($"masterPrefab is null in AssetCollection for {GetType().Name}");
            }

            if (AssetCollection.monsterCardProvider != null)
            {
                CardProvider = AssetCollection.monsterCardProvider;
            }
            else
            {
                SS2Log.Warning($"monsterCardProvider is null in AssetCollection for {GetType().Name}");
            }
        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}
