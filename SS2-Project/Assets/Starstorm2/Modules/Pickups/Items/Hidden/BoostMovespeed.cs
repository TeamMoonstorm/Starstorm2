using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections.Generic;

namespace SS2.Items
{

    //boosts movespeed by 1% per stack
    public sealed class BoostMovespeed : SS2Item
    {
        public override SS2AssetRequest<ItemDef> AssetRequest<ItemDef>() => SS2Assets.LoadAssetAsync<ItemDef>("BoostMovespeed", SS2Bundle.Items);
        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddMovespeed;
        }

        private void AddMovespeed(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.moveSpeedMultAdd += sender.GetItemCount(ItemDef) / 100f;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }
    }
}
