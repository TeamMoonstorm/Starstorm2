using R2API;
using RoR2;
using RoR2.Items;
using System;

namespace Moonstorm.Starstorm2.Items
{

    public sealed class LowQualitySpeakers : ItemBase
    {
        private const string token = "SS2_ITEM_LOWQUALITYSPEAKERS_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("LowQualitySpeakers", SS2Bundle.Items);

        [ConfigurableField(SS2Config.IDItem, ConfigName = "Maximum Move Speed per Speaker", ConfigDesc = "Maximum amount of move speed per item held.")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float bonusMoveSpeed = 0.6f;

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnIncomingDamageServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.LowQualitySpeakers;
            //★ - I love recycling code! I love recycling code!!!
            public float missingHealthPercent;
            public float healthFraction;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseMoveSpeedAdd += body.baseMoveSpeed * (1 - healthFraction) * (float)(Math.Pow(bonusMoveSpeed, 1 / stack));
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                body.RecalculateStats();
            }

            private void FixedUpdate()
            {
                missingHealthPercent = (1 - body.healthComponent.combinedHealthFraction);
                healthFraction = body.healthComponent.combinedHealthFraction - 0.1f;
                if (body.healthComponent.combinedHealthFraction > 0.9f)
                {
                    healthFraction = 1;
                }
                if (body.healthComponent.combinedHealthFraction < 0f)
                {
                    healthFraction = 0;
                }
            }
        }
    }
}
