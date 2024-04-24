using SS2.Buffs;
using RoR2;
using RoR2.Items;
using UnityEngine;
namespace SS2.Items
{
    public sealed class DetritiveTrematode : SS2Item
    {
        private const string token = "SS2_ITEM_DETRITIVETREMATODE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DetritiveTrematode", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigNameOverride = "Trematode Threshold", ConfigDescOverride = "Amount of missing health needed for Trematode to proc. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float missingHealthPercentage = 0.30f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigNameOverride = "Trematode Threshold Per Stack", ConfigDescOverride = "Increase in missing health threshold, per stack. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float missingHealthPercentagePerStack = 0.15f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Damage dealt by the Trematode debuff, per second. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float trematodeDamage = 1f;

        public static GameObject biteEffect = SS2Assets.LoadAsset<GameObject>("TrematodeBiteEffect", SS2Bundle.Items);
        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.DetritiveTrematode;

            private static BodyIndex fucker = BodyIndex.None;
            private static BodyIndex Fucker => fucker == BodyIndex.None ? BodyCatalog.FindBodyIndexCaseInsensitive("ArtifactShellBody") : fucker;
            public void OnDamageDealtServer(DamageReport report)
            {
                if (!report.victim.alive) return;

                var victim = report.victim;
                var attacker = report.attacker;
                bool hasDot = false;

                //var dotController = DotController.FindDotController(victim.gameObject);               
                //if (dotController)
                //    hasDot = dotController.HasDotActive(Trematodes.index);
                hasDot = victim.body.HasBuff(SS2Content.Buffs.BuffTrematodes);
                // dots apparently dont get updated instantly???? so we can apply multiple of the same dot before HasDotActive returns true. hopo game

                if (victim.body.bodyIndex == Fucker)
                    return;

                //30% + 15% per stack hyperbolically
                //1 = 30%
                //5 = 63.5%
                //10 = 83.8%
                float requiredHealthPercentage = 1 - (1 - missingHealthPercentage) * Mathf.Pow(1 - missingHealthPercentagePerStack, stack - 1);

                //25% + 10% per stack
                //1 = 30%
                //5 = 65%
                // 9 = 105%
                //float requiredHealthPercentage = missingHealthPercentage + missingHealthPercentagePerStack * (stack - 1);
                if (victim.combinedHealthFraction < requiredHealthPercentage && !hasDot && report.damageInfo.dotIndex != Trematodes.index && (victim.gameObject != attacker))
                {
                    
                    // do the first tick instantly
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.attacker = base.gameObject;
                    damageInfo.crit = false;
                    damageInfo.damage = trematodeDamage * base.body.damage;
                    damageInfo.force = Vector3.zero;
                    damageInfo.inflictor = base.gameObject;
                    damageInfo.position = victim.body.corePosition;
                    damageInfo.procCoefficient = 0f;
                    damageInfo.damageColorIndex = DamageColorIndex.Item;
                    damageInfo.damageType = DamageType.DoT;
                    damageInfo.dotIndex = Trematodes.index;
                    victim.TakeDamage(damageInfo);

                    var dotInfo = new InflictDotInfo()
                    {
                        attackerObject = attacker,
                        victimObject = victim.gameObject,
                        dotIndex = Trematodes.index,
                        duration = Mathf.Infinity,
                        damageMultiplier = trematodeDamage * stack,
                    };
                    DotController.InflictDot(ref dotInfo);

                    EffectManager.SimpleEffect(biteEffect, report.damageInfo.position, Quaternion.identity, true);
                }
            }
        }
    }
}
