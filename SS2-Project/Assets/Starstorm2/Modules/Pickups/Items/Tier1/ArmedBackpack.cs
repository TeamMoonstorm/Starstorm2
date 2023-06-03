using R2API;
using RoR2;
using RoR2.Items;
using RoR2.Projectile;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class ArmedBackpack : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("ArmedBackpack", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Damage dealt by the missle per stack. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_ARMEDBACKPACK_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float backpackDamageCoeff = 4f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Proc multiplier per percentage of health lost. (1 = 100% of health fraction lost)")]
        [TokenModifier("SS2_ITEM_ARMEDBACKPACK_DESC", StatTypes.Default, 1)]
        public static float procMult = 2.5f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Minimum chance for fired missile. (1 = 1% chance)")]
        [TokenModifier("SS2_ITEM_ARMEDBACKPACK_DESC", StatTypes.MultiplyByN, 2, "100")]
        public static float procMinimum = 0;

        public static ProcChainMask ignoredProcs;
        public GameObject missilePrefab;

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

                    //SS2Log.Debug("chance was: " + rollChance);
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
                        //var missleObject = GlobalEventManager.CommonAssets.missilePrefab;
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