using Moonstorm.Components;
using RoR2;
using UnityEngine;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class ExeMuteCharge : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdExeMuteCharge", SS2Bundle.Executioner2);

    }
}
