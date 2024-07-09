using MSU;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using SS2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;
using MSU.Config;
using static AkMIDIEvent;

namespace Assets.Starstorm2.Modules.Pickups.Items.Tier2
{
    //public class Balloon : SS2Item
    //{
    //    public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBalloon", SS2Bundle.Items);

    //    private const string TOKEN = "SS2_ITEM_BALLOON_DESC";

    //    //[RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of gravity removed, as a pecent")]
    //    //[FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
    //    public static float reducedGravity = 0.30f;

    //    public override void Initialize()
    //    {
    //    }

    //    public class BalloonBehavior : BaseItemBodyBehavior
    //    {
    //        //[ItemDefAssociation(useOnServer = true, useOnClient = true)]
    //       // public static ItemDef GetItemDef() => LITContent.Items.RustyJetpack;
  
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
    //                body.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * reducedGravity;

    //            }
    //        }
    //    }

    //    public override bool IsAvailable(ContentPack contentPack)
    //    {
    //        return false;
    //    }
    //}
}
