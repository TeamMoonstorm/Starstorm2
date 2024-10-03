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
    public abstract class SS2Interactable : IInteractableContentPiece, IContentPackModifier
    {
        public InteractableAssetCollection AssetCollection { get; private set; }
        public InteractableCardProvider CardProvider { get; protected set; }
        IInteractable IGameObjectContentPiece<IInteractable>.component => InteractablePrefab.GetComponent<IInteractable>();
        GameObject IContentPiece<GameObject>.asset => InteractablePrefab;
        public GameObject InteractablePrefab { get; protected set; }

        public abstract SS2AssetRequest<InteractableAssetCollection> AssetRequest { get; }

        NullableRef<InteractableCardProvider> IInteractableContentPiece.cardProvider => CardProvider;

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public virtual IEnumerator LoadContentAsync()
        {
            SS2AssetRequest<InteractableAssetCollection> request = AssetRequest;

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            AssetCollection = request.Asset;

            CardProvider = AssetCollection.interactableCardProvider;
            InteractablePrefab = AssetCollection.interactablePrefab;

        }

        public virtual void ModifyContentPack(ContentPack contentPack)
        {
            contentPack.AddContentFromAssetCollection(AssetCollection);
        }
    }
}