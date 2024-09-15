using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using SS2;
using UnityEngine;


namespace Assets.Starstorm2.Modules.Pickups.Items.Tier2
{
    public class Balloon : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acBalloon", SS2Bundle.Indev);

        private const string TOKEN = "SS2_ITEM_BALLOON_DESC";

        //[RiskOfOptionsConfigureField(LITConfig.ITEMS, ConfigDescOverride = "Amount of gravity removed, as a pecent")]
        //[FormatToken(TOKEN, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float reducedGravity = 0.50f;
        public static float stackingEffect = 0.05f;

        public override void Initialize()
        {
        }

        public class BalloonBehavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef() => SS2Content.Items.Balloon;

            private void FixedUpdate()
            {
                if (!body.characterMotor || !body)
                    return;

                if (body.characterMotor.isGrounded)
                {
                    return;
                }

                if (body.inputBank.jump.down)
                {
                    body.characterMotor.velocity.y -= Time.fixedDeltaTime * Physics.gravity.y * reducedGravity + (stackingEffect * stack);

                }
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }
    }
}
