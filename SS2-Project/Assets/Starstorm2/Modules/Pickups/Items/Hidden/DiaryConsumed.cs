using RoR2;

using Moonstorm;
namespace SS2.Items
{
    public sealed class DiaryConsumed : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DiaryConsumed", SS2Bundle.Items);
    }
}
