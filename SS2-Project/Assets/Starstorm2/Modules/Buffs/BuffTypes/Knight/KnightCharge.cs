using RoR2;
using UnityEngine;
namespace SS2.Buffs
{
    public sealed class KnightCharge : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdKnightCharged", SS2Bundle.Indev);

        public override Material OverlayMaterial { get; } = SS2Assets.LoadAsset<Material>("matKnightSuperShield", SS2Bundle.Indev);

    }
}
