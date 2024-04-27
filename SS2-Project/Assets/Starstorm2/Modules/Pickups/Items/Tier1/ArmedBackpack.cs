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
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Damage dealt by the missle per stack. (1 = 100%)")]
        [FormatToken("SS2_ITEM_ARMEDBACKPACK_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float backpackDamageCoeff = 4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Proc multiplier per percentage of health lost. (1 = 100% of health fraction lost)")]
        [FormatToken("SS2_ITEM_ARMEDBACKPACK_DESC", 1)]
        public static float procMult = 2.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Minimum chance for fired missile. (1 = 1% chance)")]
        [FormatToken("SS2_ITEM_ARMEDBACKPACK_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float procMinimum = 0;

        public static ProcChainMask ignoredProcs;


        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "ArmedBackpack" - Items
             */
            yield break;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ArmedBackpack;

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if (stack > 0 && damageReport.damageDealt > 0)
                {
                    float percentHPLoss = (damageReport.damageDealt / damageReport.victim.fullCombinedHealth) * 100f * procMult;
                    var playerBody = damageReport.victimBody;
                    var rollChance = percentHPLoss > procMinimum ? percentHPLoss : procMinimum;

                    if (Util.CheckRoll(rollChance, playerBody.master))
                    {
                        float damageCoefficient = backpackDamageCoeff * stack;
                        float missileDamage = playerBody.damage * damageCoefficient;

                        var teamIndex = damageReport.attackerTeamIndex;
                        var attacker = damageReport.attacker;

                        if (teamIndex == TeamIndex.None || teamIndex == TeamIndex.Player)
                        {
                            attacker = null; //this prevents it from firing into blood shrines and i guess yourself/teammates if that lunar active is involved
                        }
                        MissileUtils.FireMissile(
                            playerBody.corePosition,
                            playerBody,
                            ignoredProcs,
                            attacker,
                            missileDamage,
                            Util.CheckRoll(playerBody.crit, playerBody.master),
                            GlobalEventManager.CommonAssets.missilePrefab,
                            DamageColorIndex.Item,
                            false);
                    }
                }
            }
        }
    }
}