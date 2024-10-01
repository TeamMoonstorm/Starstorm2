using MSU;
using RoR2;
using RoR2.ContentManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MSU.Config;
using RoR2.Items;
using SS2.Components;
namespace SS2.Items
{

    public sealed class UraniumHoreshoe : SS2Item
    {
        private const string token = "SS2_ITEM_URANIUMHORSESHOE_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acUraniumHorseshoe", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Movement speed bonus, per stack. (1 = 100% movement speed)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100f, 0)]
        public static float movespeed = .1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Jump height bonus, per stack. (1 = 100% movement speed)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100f, 1)]
        public static float jumpBoost = .1f;       

        public override void Initialize()
        {
            R2API.RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, R2API.RecalculateStatsAPI.StatHookEventArgs args)
        {
            int count = sender.inventory ? sender.inventory.GetItemCount(SS2Content.Items.UraniumHorseshoe) : 0;
            args.moveSpeedMultAdd += movespeed * count;
            args.jumpPowerMultAdd += jumpBoost * count;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.UraniumHorseshoe;
            private ModifiedFootstepHandler footstepHandler;

            public void Start()
            {
                if (body.modelLocator && body.modelLocator.modelTransform.GetComponent<FootstepHandler>())
                {
                    footstepHandler = body.modelLocator.modelTransform.gameObject.AddComponent<ModifiedFootstepHandler>();
                    footstepHandler.footstepEffect = SS2Assets.LoadAsset<GameObject>("HorseshoeFootstep", SS2Bundle.Items);
                    footstepHandler.enableFootstepDust = true;
                    footstepHandler.rotateToMoveDirection = true;
                }
            }
            public void OnDestroy()
            {
                Destroy(footstepHandler);
            }
        }


    }
}