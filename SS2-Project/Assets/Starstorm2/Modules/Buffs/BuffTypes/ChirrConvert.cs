using Moonstorm.Components;
using R2API;
using RoR2;
using RoR2.CharacterAI;
using System.Linq;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    //[DisabledContent]
    public sealed class ChirrConvert : BuffBase, IBodyStatArgModifier
    {
        //just the dot/slow portion of Befriend. check ChirrConvertOrb for the actual converting
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffChirrConvert", SS2Bundle.Chirr);
        public static DotController.DotIndex dotIndex;

        public static float damageCoefficient = 0.8f;
        public static float slowAmount = 0.8f;

        public override void Initialize()
        {
            dotIndex = DotAPI.RegisterDotDef(.33f, damageCoefficient, DamageColorIndex.Poison, BuffDef);
        }

        public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        {
            args.moveSpeedMultAdd -= slowAmount;
        }
    }
}
