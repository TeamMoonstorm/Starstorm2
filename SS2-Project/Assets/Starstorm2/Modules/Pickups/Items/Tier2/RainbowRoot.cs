using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;

using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    public sealed class RainbowRoot : SS2Item
    {
        private const string token = "SS2_ITEM_RAINBOWROOT_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acRainbowRoot", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of armor gained upon pickup. Does not scale with stack count. (1 = 1 armor)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float baseArmor = 20;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Base portion of damage prevented to be gained as barrier. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float baseAmount = .25f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Portion of damage prevented to be gained as barrier per stack. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float scalingAmount = .15f;
      

        public override void Initialize()
        {           
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return false;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RainbowRoot;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += baseArmor;
            }

            //N: For whatever reason this was done in an On hook, which is funny. these interfaces literally exists to avoid doing that and instead use behaviours for it.
            public void OnTakeDamageServer(DamageReport damageReport)
            {
                DamageInfo damageInfo = damageReport.damageInfo;
                float redu;
                float armor = body.armor;
                float plateRedu = 0;
                if (damageInfo.rejected && body.isPlayerControlled)
                {
                    redu = damageInfo.damage;
                }
                else
                {
                    redu = armor / (armor + 100f);
                    redu *= damageInfo.damage;

                    int plate = body.healthComponent.itemCounts.armorPlate;
                    if (plate > 0)
                    {
                        float remainder = damageInfo.damage - redu;
                        plateRedu = remainder - Mathf.Max(1f, remainder - 5f * (float)plate);
                    }
                }

                float mult = MSUtil.InverseHyperbolicScaling(baseArmor, scalingAmount, 1, stack);
                //SS2Log.Info("mult: " + mult + " | reduction: " + redu + " | plateredu: " + plateRedu + " | player hp: " + self.health + " | incoming damage: " + damageInfo.damage);
                body.healthComponent.AddBarrierAuthority((redu + plateRedu) * mult);
            }
        }
    }
}
