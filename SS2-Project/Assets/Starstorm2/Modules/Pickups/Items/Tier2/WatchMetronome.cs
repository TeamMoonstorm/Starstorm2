using MSU;
using MSU.Config;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class WatchMetronome : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_WATCHMETRONOME_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acWatchMetronome", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Maximum movement speed bonus that can be achieved. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float maxMovementSpeed = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Maximum duration of the buff.")]
        [FormatToken(token, 1)]
        public static float maxDuration = 5;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Maximum duration of the buff, per stack.")]
        [FormatToken(token, 2)]
        public static float maxDurationPerStack = 3;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Duration of the buff gained per second, while not sprinting.")]
        public static float gainPerSecond = 1;
        public override void Initialize()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(SS2Content.Buffs.BuffWatchMetronome);
            if (buffCount > 0 && sender.isSprinting)
            {
                args.moveSpeedMultAdd +=  maxMovementSpeed * 0.2f * buffCount; // 0.2 = 1/5. 5 stacks for max buff
            }
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.WatchMetronome;
            private float charge;
            public void FixedUpdate()
            {
                if (!body.isSprinting)
                {
                    charge += gainPerSecond * Time.fixedDeltaTime;
                }
                else
                {
                    float drainPerSecond = maxDuration / (maxDuration + maxDurationPerStack * (stack-1));
                    charge -= drainPerSecond * Time.fixedDeltaTime;
                }
                charge = Mathf.Clamp(charge, 0, 5); // max 5 stacks of the buff

                if (NetworkServer.active)
                {
                    body.SetBuffCount(SS2Content.Buffs.BuffWatchMetronome.buffIndex, Mathf.RoundToInt(charge));
                }
            }

            private void OnDestroy()
            {
                if (NetworkServer.active)
                {
                    body.SetBuffCount(SS2Content.Buffs.BuffWatchMetronome.buffIndex, 0);
                }
            }
        }
    }
}