using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class Roulette : ItemBase
    {
        private const string token = "SS2_ITEM_JETBOOTS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Roulette", SS2Bundle.Items);

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Roulette;
            private float stopwatch;
            private static float checkTimer = 0.25f;
            private void FixedUpdate()
            {
                if (NetworkServer.active)
                {
                    stopwatch += Time.fixedDeltaTime;
                    if (stopwatch > checkTimer)
                    {
                        //float currentRouletteBuffs = body.GetBuffCount();
                    }
                }
            }
        }
    }
}
