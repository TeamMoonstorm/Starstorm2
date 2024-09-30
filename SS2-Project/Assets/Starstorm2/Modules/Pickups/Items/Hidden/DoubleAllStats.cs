using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;

namespace SS2.Items
{

    // doubles all stats per stack
    // 1 stack = 2x
    // 4 stacks = 16x

    // nevermind just health lol
    public sealed class DoubleAllStats : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("DoubleAllStats", SS2Bundle.Items);

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += DoubleStats;
        }

        private void DoubleStats(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            float itemCount = sender.inventory ? sender.inventory.GetItemCount(ItemDef) : 0;
            if (itemCount > 0)
            {
                float n = Mathf.Pow(2, itemCount);
                args.healthMultAdd *= n;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
