using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class LightningOnKill : ItemBase
    {
        private const string token = "SS2_ITEM_LIGHTNINGONKILL_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("LightningOnKill", SS2Bundle.Items);
    }
}
