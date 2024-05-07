using RoR2;
using UnityEngine;
using System.Collections.Generic;
using MSU;
namespace SS2
{
    [CreateAssetMenu(fileName = "InteractableAssetCollection", menuName = "Starstorm2/AssetCollections/InteractableAssetCollection")]
    public class InteractableAssetCollection : ExtendedAssetCollection
    {
        public GameObject interactablePrefab;
        public InteractableCardProvider interactableCardProvider;
    }
}