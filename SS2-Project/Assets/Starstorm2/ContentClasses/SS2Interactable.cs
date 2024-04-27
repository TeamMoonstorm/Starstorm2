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
        public abstract InteractableCardProvider CardProvider { get; }
        IInteractable IGameObjectContentPiece<IInteractable>.Component => InteractablePrefab.GetComponent<IInteractable>();
        GameObject IContentPiece<GameObject>.Asset => InteractablePrefab;
        public abstract GameObject InteractablePrefab { get; }

        public abstract void Initialize();
        public abstract bool IsAvailable(ContentPack contentPack);
        public abstract IEnumerator LoadContentAsync();
    }
}