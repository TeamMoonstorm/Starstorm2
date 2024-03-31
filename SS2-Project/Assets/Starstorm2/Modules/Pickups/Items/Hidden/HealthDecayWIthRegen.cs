using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using System;
using Moonstorm;
namespace SS2.Items
{
    //ror2 HealthDecay caps regen at 0. was bad for chirr
    public sealed class HealthDecayWithRegen : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("HealthDecayWithRegen", SS2Bundle.Items);

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddRegen;
        }

        private void AddRegen(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            float itemCount = sender.inventory ? sender.inventory.GetItemCount(ItemDef) : 0;
            if(itemCount > 0)
                args.baseRegenAdd -= sender.maxHealth / sender.cursePenalty / itemCount;
        }
    }
}
