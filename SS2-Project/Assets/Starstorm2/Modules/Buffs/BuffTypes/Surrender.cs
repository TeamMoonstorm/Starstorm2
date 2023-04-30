using Moonstorm.Components;
using Moonstorm.Starstorm2.Equipments;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class Surrender : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffSurrender", SS2Bundle.Indev);

        public sealed class Behavior : BaseBuffBodyBehavior, IStatItemBehavior //IBodyStatArgModifier 
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffSurrender;
            /*
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.critAdd += GreaterWarbanner.extraCrit;
                args.regenMultAdd += GreaterWarbanner.extraRegeneration;
                args.cooldownMultAdd *= GreaterWarbanner.cooldownReduction;
            }*/

            public void RecalculateStatsEnd()
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

            public void RecalculateStatsStart()
            {
            }
        }

    }
}
