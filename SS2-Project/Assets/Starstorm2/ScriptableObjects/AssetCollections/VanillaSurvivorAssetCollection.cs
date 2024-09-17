using MSU;
using RoR2;
using UnityEngine;

namespace SS2
{
    [CreateAssetMenu(fileName = "VanillaSurvivorAssetCollection", menuName = "Starstorm2/AssetCollections/VanillaSurvivorAssetCollection")]
    public class VanillaSurvivorAssetCollection : ExtendedAssetCollection
    {
        public string survivorDefAddress;

        public ParallelMultiStartCoroutine CreateCoroutineForVanillaSkinDefInitialization()
        {
            var vanillaSkinDefs = this.FindAssets<VanillaSkinDef>();
            ParallelMultiStartCoroutine coroutine = new ParallelMultiStartCoroutine();
            foreach(VanillaSkinDef skinDef in vanillaSkinDefs)
            {
                coroutine.Add(skinDef.Initialize);
            }
            return coroutine;
        }
    }
}
