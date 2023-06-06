using Moonstorm.Components;
using Moonstorm.Starstorm2.Items;
using R2API;
using RoR2;

namespace Moonstorm.Starstorm2.Buffs
{
    public sealed class PurplePoison : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("bdPurplePoison", SS2Bundle.Indev);
        //public static DotController.DotIndex index;

        public override void Initialize()
        {
            //index = DotAPI.RegisterDotDef(0.25f, 0.18f, DamageColorIndex.DeathMark, BuffDef);
        }

        public sealed class Behavior : BaseBuffBodyBehavior
        {
            [BuffDefAssociation]
            private static BuffDef GetBuffDef() => SS2Assets.LoadAsset<BuffDef>("bdPurplePoison", SS2Bundle.Indev);
        }
    }
}
