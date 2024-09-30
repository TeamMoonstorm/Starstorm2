using RoR2;
using UnityEngine;
namespace SS2
{
    [CreateAssetMenu(fileName = "BodyAssetCollection", menuName = "Starstorm2/AssetCollections/BodyAssetCollection")]
    public class BodyAssetCollection : ExtendedAssetCollection
    {
        public GameObject bodyPrefab;
        public GameObject masterPrefab;
    }
}
