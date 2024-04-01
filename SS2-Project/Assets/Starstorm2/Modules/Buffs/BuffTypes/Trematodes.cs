using R2API;
using RoR2;
using UnityEngine;
namespace SS2.Buffs
{
    public sealed class Trematodes : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffTrematodes", SS2Bundle.Items);
        public static DotController.DotIndex index;
        public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matBloodOverlay", SS2Bundle.Items);

        public override void Initialize()
        {
            index = DotAPI.RegisterDotDef(.5f, .5f, DamageColorIndex.Item, BuffDef);
        }
    }
}
