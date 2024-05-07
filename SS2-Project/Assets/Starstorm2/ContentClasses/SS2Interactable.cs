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
    /// <inheritdoc cref="IInteractableContentPiece"/>
    /// </summary>
    public abstract class SS2Interactable : IInteractableContentPiece
    {
        public InteractableAssetCollection AssetCollection { get; private set; }
        public InteractableCardProvider CardProvider;
        IInteractable IGameObjectContentPiece<IInteractable>.Component => InteractablePrefab.GetComponent<IInteractable>();
        GameObject IContentPiece<GameObject>.Asset => InteractablePrefab;
        InteractableCardProvider IInteractableContentPiece.CardProvider => CardProvider;

        public GameObject InteractablePrefab;

        public abstract SS2AssetRequest<InteractableAssetCollection> AssetRequest();
        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<InteractableAssetCollection> request = AssetRequest();

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            CardProvider = AssetCollection.interactableCardProvider;
            InteractablePrefab = AssetCollection.interactablePrefab;

            OnAssetCollectionLoaded(AssetCollection);
        }

        public virtual void OnAssetCollectionLoaded(AssetCollection assetCollection) { }
    }
}