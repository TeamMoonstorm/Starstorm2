using MSU;
using RoR2;
using System.Collections;
using UnityEngine;

namespace SS2
{
    [CreateAssetMenu(fileName = "VanillaSurvivorAssetCollection", menuName = "Starstorm2/AssetCollections/VanillaSurvivorAssetCollection")]
    public class VanillaSurvivorAssetCollection : ExtendedAssetCollection
    {
        public string survivorDefAddress;

        public IEnumerator InitializeSkinDefs()
        {
            var vanillaSkinDefs = this.FindAssets<VanillaSkinDef>();
            foreach(var skinDef in vanillaSkinDefs)
            {
                var routine = skinDef.Initialize();
                while(!routine.IsDone())
                {
                    yield return null;
                }
            }
        }
    }
}
