using Moonstorm.Components;
using Moonstorm.Starstorm2.Equipments;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class GreaterBanner : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffGreaterBanner", SS2Bundle.Equipments);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier, IStatItemBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffGreaterBanner;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                {
                    args.critAdd += GreaterWarbanner.extraCrit;
                    args.regenMultAdd += GreaterWarbanner.extraRegeneration;

                    args.cooldownReductionAdd += GreaterWarbanner.cooldownReduction;
                }
            }
            public void RecalculateStatsEnd()
            {
                if (body.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                {
                    if (body.skillLocator)
                    {
                        if (body.skillLocator.primary)
                            body.skillLocator.primary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                        if (body.skillLocator.secondary)
                            body.skillLocator.secondary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                        if (body.skillLocator.utility)
                            body.skillLocator.utility.cooldownScale *= GreaterWarbanner.cooldownReduction;
                        if (body.skillLocator.special)
                            body.skillLocator.special.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                }
            }

            public void RecalculateStatsStart()
            {

            }
            /*public void CooldownReductionWarBanner(CharacterBody OBSOLETE, RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                {
                    SkillLocator locator = body.skillLocator;

                    if (locator.primary)
                    {
                        locator.primary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                    if (locator.secondary)
                    {
                        locator.secondary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                    if (locator.utility)
                    {
                        locator.utility.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                    if (locator.special)
                    {
                        locator.special.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    }
                }
            }*/
        }

    }
}
