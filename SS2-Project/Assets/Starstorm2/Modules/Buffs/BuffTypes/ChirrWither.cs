using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using System;
using UnityEngine;
using RoR2.CharacterAI;
using System.Linq;

namespace Moonstorm.Starstorm2.Buffs
{
    public class ChirrWither : BuffBase
    {
        //SIX FUCKING BUFFS >:3
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrWither", SS2Bundle.Chirr);

        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffChirrWither;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.baseCurseAdd += 0.025f * body.GetBuffCount(SS2Content.Buffs.BuffChirrWither);
            }
        }
    }
}
