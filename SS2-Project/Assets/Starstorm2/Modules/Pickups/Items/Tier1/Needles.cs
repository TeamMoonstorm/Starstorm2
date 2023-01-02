using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class Needles : ItemBase
    {
        private const string token = "SS2_ITEM_NEEDLES_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Needles");

        [ConfigurableField(ConfigDesc = "Chance for Needles to Proc. (100 = 100%)")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float procChance = 6f;

        [ConfigurableField(ConfigDesc = "Duration of the pricked debuff, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float buildupDuration = 5f;

        [ConfigurableField(ConfigDesc = "Additional duration of the pricked debuff per stack, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float buildupStack = 1f;

        [ConfigurableField(ConfigDesc = "Amount of buildup debuffs needed before the actual needles debuff gets applied")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float neededBuildupAmount = 1f;

        [ConfigurableField(ConfigDesc = "Duration of the actual needles debuff, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float needleBuffDuration = 2f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Needles;
            public void OnDamageDealtServer(DamageReport report)
            {
                if (Util.CheckRoll((procChance * stack) * report.damageInfo.procCoefficient, body.master))
                {
                    if (!report.victimBody.HasBuff(SS2Content.Buffs.BuffNeedle))
                    {
                        report.victimBody.AddTimedBuff(SS2Content.Buffs.BuffNeedle, needleBuffDuration);
                        //report.victimBody.AddTimedBuff(SS2Content.Buffs.BuffNeedleBuildup, ((buildupDuration + (stack - 1) * buildupStack)) * report.damageInfo.procCoefficient);
                        //if (report.victimBody.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup) >= neededBuildupAmount)
                        //{
                        //    report.victimBody.ClearTimedBuffs(SS2Content.Buffs.BuffNeedleBuildup);
                        //    report.victimBody.AddTimedBuff(SS2Content.Buffs.BuffNeedle, needleBuffDuration);
                        //}
                    }
                }
            }
        }
    }
}