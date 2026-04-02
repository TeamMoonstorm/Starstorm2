using MSU;
using MSU.Config;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SS2.Items
{
    public sealed class ArmedBackpack : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acArmedBackpack", SS2Bundle.Items);
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage dealt by the missle per stack. (1 = 100%)")]
        [FormatToken("SS2_ITEM_ARMEDBACKPACK_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float backpackDamageCoeff = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Proc multiplier per percentage of health lost. (1 = 100% of health fraction lost)")]
        [FormatToken("SS2_ITEM_ARMEDBACKPACK_DESC", 1)]
        public static float procMult = 2.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Minimum chance for fired missile. (1 = 1% chance)")]
        [FormatToken("SS2_ITEM_ARMEDBACKPACK_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float procMinimum = 10;

        public static GameObject projectilePrefab;
        public static GameObject effectPrefab;

        public override void Initialize()
        {
            projectilePrefab = SS2Assets.LoadAsset<GameObject>("BackpackMissile", SS2Bundle.Items);
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ArmedBackpack;

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (stack > 0 && damageReport.damageDealt > 0)
                {
                    float victimBaseMaxHealth = damageReport.victimBody.baseMaxHealth + damageReport.victimBody.levelMaxHealth * (damageReport.victimBody.level - 1);
                    float percentHPLoss = (damageReport.damageDealt / victimBaseMaxHealth) * 100f * procMult;
                    var rollChance = percentHPLoss > procMinimum ? percentHPLoss : procMinimum;

                    if (Util.CheckRoll(rollChance, body.master))
                    {
                        float damageCoefficient = backpackDamageCoeff * stack;
                        float missileDamage = body.damage * damageCoefficient;

                        var attacker = damageReport.attacker;
                        if (damageReport.attackerBody == null)
                        {
                            attacker = null;
                        }

                        MissileUtils.FireMissile(
                            body.corePosition,
                            body,
                            default(ProcChainMask),
                            attacker,
                            missileDamage,
                            Util.CheckRoll(body.crit, body.master),
                            projectilePrefab,
                            DamageColorIndex.Item,
                            false);
                    }
                }
            }
        }
    }
}