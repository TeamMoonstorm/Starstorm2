using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;
using RoR2.ContentManagement;

namespace SS2.Items
{

    //skills recharge x% faster
    // 100 stacks = 50% cdr
    // 200 stacks = 67% cdr

    public sealed class BoostCooldowns : SS2Item
    {
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddCooldownReduction;
        }

        private void AddCooldownReduction(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            float itemCount = sender.inventory ? sender.inventory.GetItemCount(ItemDef) : 0;
            if (itemCount > 0)
            {
                args.cooldownMultAdd -= Util.ConvertAmplificationPercentageIntoReductionPercentage(itemCount) / 100f;
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = SS2Assets.LoadAssetAsync<ItemDef>("BoostCooldowns", SS2Bundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _itemDef = request.Asset;
        }
    }
}
