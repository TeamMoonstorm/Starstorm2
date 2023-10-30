using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class KnightCharge : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdKnightCharged", SS2Bundle.Indev);

        public override Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matKnightSuperShield", SS2Bundle.Indev);

    }
}
