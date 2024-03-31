using Moonstorm.Components;
using R2API;
using RoR2;
using System;

using Moonstorm;
namespace SS2.Buffs
{
    public sealed class CoffeeBag : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffCoffeeBag", SS2Bundle.Items);
    }
}
