using R2API;
using RoR2;
using RoR2.Items;

using MSU;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;

namespace SS2.Items
{
    public sealed class NemesisBossHelper : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("NemBossHelper", SS2Bundle.Items);
        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if(sender.HasItem(ItemDef))
            {
                args.healthMultAdd += 12;
                args.damageMultAdd += 0.75f;
                args.regenMultAdd = 0;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
