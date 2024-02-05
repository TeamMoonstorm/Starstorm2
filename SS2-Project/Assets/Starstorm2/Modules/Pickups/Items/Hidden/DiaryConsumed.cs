using RoR2;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class DiaryConsumed : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("DiaryConsumed", SS2Bundle.Items);
    }
}
