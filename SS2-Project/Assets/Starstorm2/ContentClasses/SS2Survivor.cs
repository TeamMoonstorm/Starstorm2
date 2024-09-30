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
    /// <inheritdoc cref="ISurvivorContentPiece"/>
    /// </summary>
    public abstract class SS2Survivor : ISurvivorContentPiece, IContentPackModifier
    {
        public  SurvivorAssetCollection AssetCollection { get; set; }
        public SurvivorDef survivorDef { get; protected  set; }
        public NullableRef<GameObject> masterPrefab { get; protected set; }
        CharacterBody IGameObjectContentPiece<CharacterBody>.component => CharacterPrefab.GetComponent<CharacterBody>();
        GameObject IContentPiece<GameObject>.asset => CharacterPrefab;
        public GameObject CharacterPrefab { get; protected set; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);

        public abstract SS2AssetRequest<SurvivorAssetCollection> AssetRequest { get; }

        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<SurvivorAssetCollection> request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            CharacterPrefab = AssetCollection.bodyPrefab;
            masterPrefab = AssetCollection.masterPrefab;
            survivorDef = AssetCollection.survivorDef;

        }


        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}