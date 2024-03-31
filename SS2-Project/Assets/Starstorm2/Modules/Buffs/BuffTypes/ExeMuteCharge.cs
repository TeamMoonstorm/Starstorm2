using Moonstorm.Components;
using RoR2;
using UnityEngine;

using Moonstorm;
namespace SS2.Buffs
{
    public sealed class ExeMuteCharge : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdExeMuteCharge", SS2Bundle.Executioner2);

    }
}
