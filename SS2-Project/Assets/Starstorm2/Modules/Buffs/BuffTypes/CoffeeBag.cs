using Moonstorm.Components;
using R2API;
using RoR2;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class CoffeeBag : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffCoffeeBag", SS2Bundle.Items);
    }
}
