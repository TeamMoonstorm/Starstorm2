﻿using Moonstorm.Starstorm2.Buffs;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class DetritiveTrematode : ItemBase
    {
        private const string token = "SS2_ITEM_DETRITIVETREMATODE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DetritiveTrematode");

        [ConfigurableField(ConfigName = "Trematode Threshold", ConfigDesc = "Amount of missing health needed for Trematode to proc. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 0)]
        public static float missingHealthPercentage = 0.4f;

        [ConfigurableField(ConfigDesc = "Movement speed reduction received from the Trematode debuff. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 2)]
        public static float trematodeSlow = 0.2f;

        [ConfigurableField(ConfigDesc = "Duration of Trematode debuff, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float dotDuration = 4;

        [ConfigurableField(ConfigDesc = "Damage dealt by the Trematode debuff, per second. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 1)]
        public static float trematodeDamage = 1f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DetritiveTrematode;
            public void OnDamageDealtServer(DamageReport report)
            {
                var victim = report.victim;
                var attacker = report.attacker;
                var dotController = DotController.FindDotController(victim.gameObject);
                bool hasDot = false;
                if (dotController)
                    hasDot = dotController.HasDotActive(Trematodes.index);

                if (victim.combinedHealthFraction < missingHealthPercentage && !hasDot && (victim.gameObject != attacker))
                {
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attacker,
                        victimObject = victim.gameObject,
                        dotIndex = Trematodes.index,
                        duration = report.damageInfo.procCoefficient * dotDuration,
                        damageMultiplier = trematodeDamage * stack,
                    };
                    DotController.InflictDot(ref dotInfo);
                }
            }
        }
    }
}
