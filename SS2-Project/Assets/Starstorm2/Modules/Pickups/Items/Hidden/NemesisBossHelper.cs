using R2API;
using RoR2;
using RoR2.Items;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class NemesisBossHelper : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("NemBossHelper", SS2Bundle.Items);

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.NemBossHelper;
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.healthMultAdd += 12;
                args.damageMultAdd += 0.75f;
                body.regen = 0;                     //★ Is it wrong to directly modify the stat like this when I want to set it to 0?
            }                                       //★ ...Is it even worth commenting here? I'm alone now.
        }                                           //...I'm not signing for anyone but myself, either.
    }                                               // Neb - Hi Swuff!! :D
}
