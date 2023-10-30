using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class KnightBuff : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdKnightBuff", SS2Bundle.Indev);

        public override Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matKnightBuffOverlay", SS2Bundle.Indev);
        public sealed class Behavior : BaseBuffBodyBehavior, IBodyStatArgModifier
        {
            private ModelLocator ml;
            private Transform mt;
            private CharacterModel cm;
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.bdKnightBuff;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.attackSpeedMultAdd += 0.4f;
                args.moveSpeedMultAdd += 0.4f;
            }
        }
    }
}
