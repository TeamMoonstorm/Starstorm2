using MSU;
using R2API;
using RoR2;
using System.Collections;
using UnityEngine;

namespace SS2.Items
{

    //boosts movespeed by 1% per stack
    public sealed class BoostMovespeed : SS2Item
    {
        public override NullableRef<GameObject> ItemDisplayPrefab => null;
        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += AddMovespeed;
        }

        private void AddMovespeed(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.moveSpeedMultAdd += sender.GetItemCount(_itemDef) / 100f;
        }

        public override bool IsAvailable()
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            var request = SS2Assets.LoadAssetAsync<ItemDef>("BoostMovespeed", SS2Bundle.Items);

            request.StartLoad();
            while (!request.IsComplete)
                yield return null;

            _itemDef = request.Asset;
        }
    }
}
