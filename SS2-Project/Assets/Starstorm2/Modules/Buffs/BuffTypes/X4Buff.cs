using Moonstorm.Components;
using R2API;
using RoR2;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class X4Buff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffX4", SS2Bundle.Items);

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
                    int count = body.GetItemCount(SS2Content.Items.X4);
                    float regenAmnt = Items.X4.baseRegenBoost + (Items.X4.stackRegenBoost * (count - 1));
                    args.baseRegenAdd += (regenAmnt + ((regenAmnt / 5) * body.level)) * buffStacks; //original code not taken from bitter root becasue i was lazy
                    //SS2Log.Debug(regenAmnt + " per buff stack");
                }
            }
        }
    }
}
