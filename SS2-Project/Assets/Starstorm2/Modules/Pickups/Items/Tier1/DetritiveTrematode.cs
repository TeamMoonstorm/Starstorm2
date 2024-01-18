using Moonstorm.Starstorm2.Buffs;
using RoR2;
using RoR2.Items;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class DetritiveTrematode : ItemBase
    {
        private const string token = "SS2_ITEM_DETRITIVETREMATODE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DetritiveTrematode", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigName = "Trematode Threshold", ConfigDesc = "Amount of missing health needed for Trematode to proc. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float missingHealthPercentage = 0.25f;

        [RooConfigurableField(SS2Config.IDItem, ConfigName = "Trematode Threshold Per Stack", ConfigDesc = "Increase in missing health threshold, per stack. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float missingHealthPercentagePerStack = 0.1f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Damage dealt by the Trematode debuff, per second. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float trematodeDamage = 1f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Movement speed reduction received from the Trematode debuff. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 3, "100")]
        public static float trematodeSlow = 0.2f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DetritiveTrematode;

            private static BodyIndex fucker = BodyIndex.None;
            private static BodyIndex Fucker => fucker == BodyIndex.None ? BodyCatalog.FindBodyIndexCaseInsensitive("ArtifactShellBody") : fucker;
            public void OnDamageDealtServer(DamageReport report)
            {
                var victim = report.victim;
                var attacker = report.attacker;
                var dotController = DotController.FindDotController(victim.gameObject);
                bool hasDot = false;
                if (dotController)
                    hasDot = dotController.HasDotActive(Trematodes.index);

                if (victim.body.bodyIndex == Fucker)
                    return;

                //25% + 10% per stack hyperbolically
                //1 = 25%
                //5 = 50.7%
                //10 = 70.9%
                //float requiredHealthPercentage = 1 - (1 - missingHealthPercentage) * Mathf.Pow(1 - missingHealthPercentagePerStack, stack - 1);

                //25% + 10% per stack
                //1 = 25%
                //5 = 65%
                // 9 = 105%
                float requiredHealthPercentage = missingHealthPercentage + missingHealthPercentagePerStack * stack;
                if (victim.combinedHealthFraction < requiredHealthPercentage && !hasDot && (victim.gameObject != attacker))
                {
                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attacker,
                        victimObject = victim.gameObject,
                        dotIndex = Trematodes.index,
                        duration = -1f,
                        damageMultiplier = trematodeDamage * stack,
                    };
                    DotController.InflictDot(ref dotInfo);
                }
            }
        }
    }
}
