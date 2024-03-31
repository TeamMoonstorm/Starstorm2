using RoR2;
using Moonstorm;
namespace SS2.Buffs
{
    public sealed class JetBootsReady : BuffBase // do we really need a class for fuicking everything?
    {
        public override BuffDef BuffDef { get; } = SS2Assets.LoadAsset<BuffDef>("BuffJetBootsReady", SS2Bundle.Items);
    }
}