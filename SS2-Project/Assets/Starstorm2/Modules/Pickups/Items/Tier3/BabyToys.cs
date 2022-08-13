using R2API;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class BabyToys : ItemBase
    {
        private const string token = "SS2_ITEM_BABYTOYS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("BabyToys");

        [ConfigurableField(ConfigName = "Stat Multiplier", ConfigDesc = "Multiplier applied to the stats per stack.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        [TokenModifier(token, StatTypes.DivideBy2, 1)]
        public static float StatMultiplier = 3;

        [ConfigurableField(ConfigName = "XP Multiplier", ConfigDesc = "Multiplier applied to XP Gain per stack.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        [TokenModifier(token, StatTypes.DivideBy2, 3)]
        public static float XPMultiplier = 2;

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnKilledOtherServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.BabyToys;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.armorAdd += GetStatAugmentation(body.levelArmor);
                args.baseAttackSpeedAdd += GetStatAugmentation(body.levelAttackSpeed);
                args.baseDamageAdd += GetStatAugmentation(body.levelDamage);
                args.baseHealthAdd += GetStatAugmentation(body.levelMaxHealth);
                args.baseMoveSpeedAdd += GetStatAugmentation(body.levelMoveSpeed);
                args.baseRegenAdd += GetStatAugmentation(body.levelRegen);
                args.baseShieldAdd += GetStatAugmentation(body.levelMaxShield);
                args.critAdd += GetStatAugmentation(body.levelCrit);
            }

            private float GetStatAugmentation(float stat)
            {
                return stat * (StatMultiplier + ((StatMultiplier / 2) * (stack - 1)));
            }
            public void OnKilledOtherServer(DamageReport damageReport)
            {
                if (damageReport.victimBody)
                {
                    var deathRewards = damageReport.victimBody.GetComponent<DeathRewards>();
                    if (deathRewards)
                    {
                        body.master.GiveExperience((ulong)(deathRewards.expReward * (XPMultiplier + ((XPMultiplier / 2) * (stack - 1)))));
                    }
                }
            }
        }
    }
}