using MSU;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class AffixStorm : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acAffixStorm", SS2Bundle.Equipments);

        public override void Initialize()
        {

        }
        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class BodyBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.AffixStorm;

            public void Start()
            {
                if (NetworkServer.active)
                {
                    body.AddBuff(SS2Content.Buffs.BuffAffixStorm);


                    // FOR TESTING, JUST TO MAKE THEM STRONGER. MAKE A REAL EFFECT LATER(?)
                    body.inventory.GiveItem(SS2Content.Items.BoostCooldowns, 30);
                    body.inventory.GiveItem(SS2Content.Items.BoostMovespeed, 30);
                    body.inventory.GiveItem(RoR2Content.Items.BoostHp, 30);
                }
            }

            private void OnDestroy()
            {
                if (NetworkServer.active && body.enabled)
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffAffixStorm);
                }
            }

        }
    }
}
