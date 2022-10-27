using Moonstorm.Components;
using RoR2;
using System;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class CoffeeBag : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffCoffeeBag");

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffCoffeeBag;
            private void Start()
            {
                
            }
        }
    }
}
