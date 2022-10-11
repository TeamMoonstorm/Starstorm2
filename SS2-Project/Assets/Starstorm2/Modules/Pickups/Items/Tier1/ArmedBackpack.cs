using RoR2;
using RoR2.Items;
using System.Linq;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class ArmedBackpack : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("ArmedBackpack");

        [ConfigurableField(ConfigDesc = "Damage dealt by the backpack. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_ARMEDBACKPACK_DESC", StatTypes.Percentage, 0)]
        public static float backpackDamage = 1f;

        [ConfigurableField(ConfigDesc = "Proc multiplier per percentage of health lost. (1 = 100% of health fraction lost)")]
        [TokenModifier("SS2_ITEM_ARMEDBACKPACK_DESC", StatTypes.Percentage, 0)]
        public static float procMult = 2f;

        public static ProcChainMask ignoredProcs;

        public sealed class Behavior : BaseItemBodyBehavior, IOnTakeDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.ArmedBackpack;

            public void OnTakeDamageServer(DamageReport damageReport)
            {
                if(stack > 0 && damageReport.damageDealt > 0)
                {
                    float percentHPLoss = (damageReport.damageDealt / damageReport.victim.fullCombinedHealth) * 100f * procMult;
                    var playerBody = damageReport.victimBody;

                    if (Util.CheckRoll(percentHPLoss, playerBody.master))
                    {
                        float damageCoefficient = backpackDamage * stack;
                        float missileDamage = playerBody.damage * damageCoefficient;

                        var teamIndex = damageReport.attackerTeamIndex;
                        var attacker = damageReport.attacker;
                        
                        if (teamIndex == TeamIndex.None || teamIndex == TeamIndex.Player )
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
                            true);
                    }
                }
            }
        }
    }
}
