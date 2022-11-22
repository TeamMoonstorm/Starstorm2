using Moonstorm.Components;
using R2API;
using RoR2;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class X4Buff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffX4");

        public sealed class X4BuffBehavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]

            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffX4;

            //private float atkMult = Items.X4.atkSpeedBonus;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //args.attackSpeedMultAdd += atkMult;
                if (body.HasBuff(SS2Content.Buffs.BuffX4))
                {
                    args.baseRegenAdd += (Items.X4.baseRegenBoost + ((Items.X4.baseRegenBoost / 5) * body.level)) * buffStacks; //original code not taken from bitter root becasue i was lazy
                }
            }
        }
    }
}
