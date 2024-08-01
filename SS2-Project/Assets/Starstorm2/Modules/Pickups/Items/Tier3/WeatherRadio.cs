using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class WeatherRadio : SS2Item, IContentPackModifier
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acWeatherRadio", SS2Bundle.Items);
        public override void Initialize()
        {
            // ONSTARTSTORM += BUFF
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

    }
}
