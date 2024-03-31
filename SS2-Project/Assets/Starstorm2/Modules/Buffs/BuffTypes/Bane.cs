using Moonstorm.Components;
using SS2.Items;
using R2API;
using RoR2;

using Moonstorm;
namespace SS2.Buffs
{
    public sealed class Bane : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffBane", SS2Bundle.Items);
        public static DotController.DotIndex index;

        public override void Initialize()
        {
            index = DotAPI.RegisterDotDef(1, .15f, DamageColorIndex.Poison, BuffDef);
        }
    }
}
