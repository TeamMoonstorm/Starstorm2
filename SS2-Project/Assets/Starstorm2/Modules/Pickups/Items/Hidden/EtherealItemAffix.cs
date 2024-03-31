using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using System;
using UnityEngine.Networking;

using Moonstorm;
namespace SS2.Items
{
    public sealed class EtherealItemAffix : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("EtherealItemAffix", SS2Bundle.Equipments);

        public override void Initialize()
        {

        }
        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.EtherealItemAffix;

            public void Start()
            {
                if (NetworkServer.active)
                {
                    body.AddBuff(SS2Content.Buffs.bdEthereal);
                }
            }

            private void OnDestroy()
            {
                if (NetworkServer.active && body.enabled)
                {
                    body.RemoveBuff(SS2Content.Buffs.bdEthereal);
                }
            }
        }
    }
}
