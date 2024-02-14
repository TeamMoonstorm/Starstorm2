using Moonstorm.Components;
using R2API;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Moonstorm.Components.BaseBuffBodyBehavior;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class KnightSpecialPowerBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdKnightSpecialPowerBuff", SS2Bundle.Indev);

        public override Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matKnightBuffOverlay", SS2Bundle.Indev);
        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightSpecialPowerBuff;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseJumpPowerAdd += 0.7f;
                args.baseMoveSpeedAdd += 0.4f;
            }
        }
    }
}
