using RoR2;
using RoR2.Items;
using UnityEngine;

using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;

namespace SS2.Items
{
    // needs sound
    public sealed class Needles : SS2Item
    {
        private const string token = "SS2_ITEM_NEEDLES_DESC";
        public override SS2AssetRequest<ItemAssetCollection> AssetRequest<ItemAssetCollection>()
        {
            return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acNeedles", SS2Bundle.Items);
        }
        public override void OnAssetCollectionLoaded(AssetCollection assetCollection)
        {
            _procEffect = assetCollection.FindAsset<GameObject>("NeedlesProcEffect");
            _critEffect = assetCollection.FindAsset<GameObject>("NeedlesCritEffect");
        }

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Chance for the debuff to be applied on hit. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float procChance = 4;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of guaranteed critical hits per stack of this item. (1 = 1 critical hit per stack before the buff is cleared)")]
        [FormatToken(token, 1)]
        public static int critsPerStack = 3;

        private static GameObject _procEffect;
        private static GameObject _critEffect;

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        // should just be an ilhook but im lazy
        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Needles;
            public void OnIncomingDamageOther(HealthComponent self, DamageInfo damageInfo)
            {
                if (damageInfo.rejected || damageInfo.damageType.HasFlag(DamageType.DoT)) return;

                //needles can only proc once crits are depleted
                bool hasBuff = self.body.HasBuff(SS2Content.Buffs.BuffNeedleBuildup);
                if (!damageInfo.crit && hasBuff)
                {
                    damageInfo.crit = true;
                    self.body.RemoveBuff(SS2Content.Buffs.BuffNeedleBuildup);

                    EffectManager.SimpleEffect(_critEffect, damageInfo.position, Quaternion.identity, true);
                }

                if (!hasBuff && Util.CheckRoll(procChance * damageInfo.procCoefficient, body.master))
                {
                    int needlesStacks = body.master.inventory.GetItemCount(SS2Content.Items.Needles);
                    for (int i = 0; i < critsPerStack * needlesStacks; i++)
                        self.body.AddBuff(SS2Content.Buffs.BuffNeedleBuildup);

                    EffectManager.SimpleEffect(_procEffect, damageInfo.position, Quaternion.identity, true);
                }
            }
        }
    }
}