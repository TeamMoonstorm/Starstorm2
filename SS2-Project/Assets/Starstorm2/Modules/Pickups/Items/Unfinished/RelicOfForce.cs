using R2API;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class RelicOfForce : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfForce");

        [ConfigurableField(ConfigDesc = "Damage multipler per stack. (1 = 100% more damage, multiplicatively)")]
        [TokenModifier("SS2_ITEM_RELICOFFORCE_DESC", StatTypes.Percentage, 0)]
        public static float damageMultiplier = 1;

        [ConfigurableField(ConfigDesc = "Attack speed reduction and cooldown increase per stack. (1 = 100% slower attack speed and longer cooldowns)")]
        [TokenModifier("SS2_ITEM_RELICOFFORCE_DESC", StatTypes.Percentage, 1)]
        public static float forcePenalty = .4f;


        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfForce;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.damageMultAdd += damageMultiplier;
                float penalty = MSUtil.InverseHyperbolicScaling(forcePenalty, forcePenalty, 0.9f, stack);
                args.attackSpeedMultAdd -= penalty;
                args.cooldownMultAdd += penalty;
            }
            public void RecalculateStatsEnd()
            {
                if (body.skillLocator)
                {
                    float cooldownPenalty = MSUtil.InverseHyperbolicScaling(forcePenalty, forcePenalty, 0.9f, stack);
                    SS2Log.Debug("Relic Stats - " + cooldownPenalty);
                    if (body.skillLocator.primary)
                    {
                        body.skillLocator.primary.flatCooldownReduction += 0;
                        body.skillLocator.primary.cooldownScale *= cooldownPenalty;
                    }
                    if (body.skillLocator.secondary)
                    {
                        body.skillLocator.secondary.flatCooldownReduction += 0;
                        body.skillLocator.secondary.cooldownScale *= cooldownPenalty;
                    }
                    if (body.skillLocator.utility)
                    {
                        body.skillLocator.utility.flatCooldownReduction += 0;
                        body.skillLocator.utility.cooldownScale *= cooldownPenalty;
                    }
                    if (body.skillLocator.special)
                    {
                        body.skillLocator.special.flatCooldownReduction += 0;
                        body.skillLocator.special.cooldownScale *= cooldownPenalty;
                    }
                        
                }
            }

            public void RecalculateStatsStart()
            {
                
            }
            private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
            {
            }
        }
    }
}
