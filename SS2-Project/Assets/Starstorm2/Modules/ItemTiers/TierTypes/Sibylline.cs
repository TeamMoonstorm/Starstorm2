using Moonstorm.Experimental;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Moonstorm.Starstorm2.ItemTiers
{
    public class Sibylline : ItemTierBase
    {
        public override ItemTierDef ItemTierDef => SS2Assets.LoadAsset<ItemTierDef>("Sibylline", SS2Bundle.Items);

        public override GameObject PickupDisplayVFX => SS2Assets.LoadAsset<GameObject>("SibyllinePickupDisplayVFX", SS2Bundle.Items);

        public override void Initialize()
        {
            base.Initialize();
            //ItemTierDef.highlightPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/UI/HighlightTier1Item.prefab").WaitForCompletion();
            ItemTierDef.dropletDisplayPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Common/VoidOrb.prefab").WaitForCompletion();
        }
    }
}
