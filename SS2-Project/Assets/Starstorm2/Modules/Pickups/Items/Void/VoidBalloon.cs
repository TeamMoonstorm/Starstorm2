using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using SS2;
using System;
using System.Collections.Generic;
using UnityEngine;


namespace SS2.Items
{
    //public class VoidBalloon : SS2VoidItem
    //{
    //    private const string TOKEN = "SS2_ITEM_VOID_BALLOON_DESC";

    //    public override void Initialize()
    //    {
    //    }

    //    public class VoidBalloonBehavior : BaseItemBodyBehavior
    //    {
    //        //[ItemDefAssociation(useOnServer = true, useOnClient = true)]
    //        public static ItemDef GetItemDef() => SS2Content.Items.VoidBalloon;

    //        private void FixedUpdate()
    //        {
    //            if (!body.characterMotor || !body)
    //                return;

    //            if (body.characterMotor.isGrounded)
    //            {
    //                return;
    //            }

    //            if (body.inputBank.jump.down)
    //            {
    //                if (body.characterMotor.velocity.y < 0)
    //                {
    //                    // This might be jank, so maybe we just make it super close to 0
    //                    body.characterMotor.velocity.y = 0;
    //                }
    //            }
    //        }
    //    }

    //    public override bool IsAvailable(ContentPack contentPack)
    //    {
    //        return true;
    //    }

    //    public override List<ItemDef> GetInfectableItems()
    //    {
    //        return new List<ItemDef>
    //        {
    //            SS2Content.Items.Balloon
    //        };
    //    }

    //    public override SS2AssetRequest AssetRequest()
    //    {
    //        return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acVoidBalloon", SS2Bundle.Items);
    //    }
    //}
}