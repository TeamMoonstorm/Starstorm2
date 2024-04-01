using RoR2;
namespace SS2.Buffs
{
    public sealed class BuffTerminationReady : BuffBase
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffTerminationReady", SS2Bundle.Items);

        //public override Material OverlayMaterial => SS2Assets.LoadAsset<Material>("matTerminationOverlay");
    }
}
