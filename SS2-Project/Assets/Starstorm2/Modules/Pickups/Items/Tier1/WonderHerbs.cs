using RoR2;

using MSU;
using System.Collections.Generic;
using UnityEngine;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
#if DEBUG
    public sealed class WonderHerbs : SS2Item
    {
        public override SS2AssetRequest<ItemAssetCollection> AssetRequest<ItemAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acWonderHerbs", SS2Bundle.Items);
        }


        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Bonus healing per herbs. (1 = 100%)")]
        [FormatToken("SS2_ITEM_FORK_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float healBonus = 0.8f;
        public override void Initialize()
        {
            HealthComponent.onCharacterHealServer += BonusHeals;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            throw new System.NotImplementedException();
        }

        private void BonusHeals(HealthComponent healthComponent, float healAmount, ProcChainMask procChainMask)
        {
            if(healthComponent.body.TryGetItemCount(ItemDef, out var itemCount))
            {
                healAmount *= 1f + MSUtil.InverseHyperbolicScaling(healBonus, healBonus, 0.6f, itemCount);
            }
        }
    }
#endif
}
