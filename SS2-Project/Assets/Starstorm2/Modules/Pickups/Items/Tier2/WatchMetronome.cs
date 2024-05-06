using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace SS2.Items
{
    public sealed class WatchMetronome : SS2Item
    {
        private const string token = "SS2_ITEM_WATCHMETRONOME_DESC";
        public override SS2AssetRequest<ItemAssetCollection> AssetRequest<ItemAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acWatchMetronome", SS2Bundle.Items);
        }

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of max charges per stack.")]
        [FormatToken(token, 0)]
        public static int chargeAmount = 5;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Maximum movement speed bonus that can be achieved via metronome per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float maxMovementSpeed = 2;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.WatchMetronome;
            //this gets halved if they are walking
            private const float buildCoefficient = 0.875f;
            private const float drainCoefficient = -1f;
            private const float notMoveBuffer = 0.6f;

            private float charges;


            public void FixedUpdate()
            {
                //What the fuck? To-do: Full rewrite this.

                //N: full rewrite my ass, this doesnt need a full rewrite you dummy.
                float metronomeCharge = Time.fixedDeltaTime * 0.75f * stack;
                if (!body.isSprinting)
                {
                    metronomeCharge *= buildCoefficient;

                }
                else
                {
                    metronomeCharge *= drainCoefficient;
                }
                charges = Mathf.Clamp(charges + metronomeCharge, 0, stack * chargeAmount);

                if (NetworkServer.active)
                {
                    body.SetBuffCount(SS2Content.Buffs.BuffWatchMetronome.buffIndex, Mathf.RoundToInt(charges));
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