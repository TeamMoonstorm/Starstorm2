using Moonstorm.Components;
using Moonstorm.Starstorm2.Equipments;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class GreaterBanner : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffGreaterBanner");

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier, IStatItemBehavior 
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffGreaterWarbanner;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += GreaterWarbanner.extraCrit;
                args.regenMultAdd += GreaterWarbanner.extraRegeneration;
               
            }

            public void RecalculateStatsEnd()
            {
                if (body.skillLocator)
                {
                    if (body.skillLocator.primary)
                        body.skillLocator.primary.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    if (body.skillLocator.secondaryBonusStockSkill)
                        body.skillLocator.secondaryBonusStockSkill.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    if (body.skillLocator.utilityBonusStockSkill)
                        body.skillLocator.utilityBonusStockSkill.cooldownScale *= GreaterWarbanner.cooldownReduction;
                    if (body.skillLocator.specialBonusStockSkill)
                        body.skillLocator.specialBonusStockSkill.cooldownScale *= GreaterWarbanner.cooldownReduction;

                }
            }

            public void RecalculateStatsStart()
            {
            }
        }

    }
}
