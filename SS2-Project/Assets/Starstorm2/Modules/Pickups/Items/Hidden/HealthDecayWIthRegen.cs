using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS2.Items
{
    //ror2 HealthDecay caps regen at 0. was bad for chirr
    public sealed class HealthDecayWithRegen : SS2Item
    {
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddRegen;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = SS2Assets.LoadAssetAsync<ItemDef>("HealthDecayWithRegen", SS2Bundle.Chirr);
            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _itemDef = request.Asset;
        }

        private void AddRegen(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            float itemCount = sender.inventory ? sender.inventory.GetItemCount(ItemDef) : 0;
            if(itemCount > 0)
                args.baseRegenAdd -= sender.maxHealth / sender.cursePenalty / itemCount;
        }
    }
}
