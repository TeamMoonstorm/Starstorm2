using MSU;
using R2API;
using RoR2;
using RoR2.Items;
using RoR2.ContentManagement;
using UnityEngine;

namespace SS2.Items
{

    //boosts movespeed by 1% per stack
    public sealed class AtkSpeedDecreasesUtilCooldown : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemDef>("AtkSpeedDecreasesUtilCooldown", SS2Bundle.Indev);

        public static float cdReduction = .1f;

        public override void Initialize() { }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public sealed class Behavior : BaseItemBodyBehavior, IStatItemBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.AtkSpeedDecreasesUtilCooldown;

            public void RecalculateStatsEnd()
            {
                if (body.skillLocator)
                {
                    if (body.skillLocator.utility)
                    {
                        float timeMult = Mathf.Pow(1 - cdReduction, stack);
                        body.skillLocator.secondaryBonusStockSkill.cooldownScale *= timeMult;
                    }
                }
            }

            public void RecalculateStatsStart(){ }
        }
    }
}
