using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using System;
using Moonstorm;
namespace SS2.Items
{

    //boosts movespeed by 1% per stack
    public sealed class BoostMovespeed : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BoostMovespeed", SS2Bundle.Items);

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddMovespeed;
        }

        private void AddMovespeed(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            float itemCount = sender.inventory ? sender.inventory.GetItemCount(ItemDef) : 0;
            if (itemCount > 0)
                args.moveSpeedMultAdd += itemCount / 100f;
        }
    }
}
