using R2API;
using RoR2;
using RoR2.Items;

using MSU;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class RelicOfMass : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acRelicOfMass", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of health increase. (1 = 100%)")]
        [FormatToken("SS2_ITEM_RELICOFMASS_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float healthIncrease = 1f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of which acceleration is divided by.")]
        [FormatToken("SS2_ITEM_RELICOFMASS_DESC", 1)]
        public static float acclMult = 8f;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        //N: Maybe this can be reduced to just a RecalcStatsAPI call? that'd be ideal.
        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier, IStatItemBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfMass;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseHealthAdd += (body.baseMaxHealth + (body.levelMaxHealth * (body.level - 1))) * (stack * healthIncrease);
                //args.moveSpeedMultAdd += stack / 2;
            }
            public void RecalculateStatsStart()
            {

            }

            public void RecalculateStatsEnd()
            {
                body.acceleration = body.baseAcceleration / (stack * acclMult);
            }
        }
    }
}
