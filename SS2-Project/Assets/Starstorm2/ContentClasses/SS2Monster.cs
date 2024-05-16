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

namespace SS2
{
    /// <summary>
    /// <inheritdoc cref="IMonsterContentPiece"/>
    /// </summary>
    public abstract class SS2Monster : IMonsterContentPiece, IContentPackModifier
    {
        public NullableRef<MonsterCardProvider> CardProvider { get; protected set; }
        public NullableRef<DirectorAPI.DirectorCardHolder> DissonanceCard { get; protected set; }
        public MonsterAssetCollection AssetCollection { get; private set; }
        public NullableRef<GameObject> MasterPrefab { get; protected set; }

        NullableRef<DirectorAPI.DirectorCardHolder> IMonsterContentPiece.DissonanceCard => DissonanceCard;
        CharacterBody IGameObjectContentPiece<CharacterBody>.Component => CharacterPrefab.GetComponent<CharacterBody>();
        NullableRef<MonsterCardProvider> IMonsterContentPiece.CardProvider => CardProvider;
        GameObject IContentPiece<GameObject>.Asset => CharacterPrefab;
        public GameObject CharacterPrefab { get; private set; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public abstract SS2AssetRequest<MonsterAssetCollection> AssetRequest { get; }

        

        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<MonsterAssetCollection> request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            CharacterPrefab = AssetCollection.bodyPrefab;
            MasterPrefab = AssetCollection.masterPrefab;
            CardProvider = AssetCollection.monsterCardProvider;
            DissonanceCard = AssetCollection.dissonanceCardHolder;
        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}
