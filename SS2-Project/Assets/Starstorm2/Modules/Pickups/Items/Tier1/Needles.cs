using RoR2;
using RoR2.Items;
using UnityEngine;

using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System;

namespace SS2.Items
{
    // needs sound
    public sealed class Needles : SS2Item
    {
        private const string token = "SS2_ITEM_NEEDLES_DESC";
        public override NullableRef<List<GameObject>> ItemDisplayPrefabs => null;

        public override ItemDef ItemDef => _itemDef;
        private ItemDef _itemDef;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Chance for the debuff to be applied on hit. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float procChance = 4;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Amount of guaranteed critical hits per stack of this item. (1 = 1 critical hit per stack before the buff is cleared)")]
        [FormatToken(token, 1)]
        public static int critsPerStack = 3;

        private static GameObject _procEffect;
        private static GameObject _critEffect;

        private BuffDef _buffNeedleBuildup; //{ get; } = SS2Assets.LoadAsset<BuffDef>("BuffNeedleBuildup", SS2Bundle.Items);
        private BuffDef _buffNeedle; //{ get; } = SS2Assets.LoadAsset<BuffDef>("BuffNeedle", SS2Bundle.Items);

        public override void Initialize()
        {
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override IEnumerator LoadContentAsync()
        {
            /*
             * ItemDef - "Needles" - Items
             * GameObject - "NeedlesProcEffect" - Items
             * GameObject - "NeedlesCritEffect" - Items
             * BuffDef - "BuffNeedleBuildup" - Items
             * BuffDef - "BuffNeedle" - Items
             */
            yield break;
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

        public sealed class Needle : BaseBuffBehaviour, RoR2.IOnIncomingDamageServerReceiver
        {
            [BuffDefAssociation]
            public static BuffDef GetBuffDef() => SS2Content.Buffs.BuffNeedle;

            private void Start()
            {
                if (CharacterBody.healthComponent)
                    HG.ArrayUtils.ArrayAppend(ref CharacterBody.healthComponent.onIncomingDamageReceivers, this);
            }
            public void OnIncomingDamageServer(DamageInfo info)
            {
                info.crit = true;
            }

            private void OnDestroy()
            {
                //This SHOULDNT cause any errors because nothing should be fucking with the order of things in this list... I hope.
                if (CharacterBody.healthComponent)
                {
                    int i = Array.IndexOf(CharacterBody.healthComponent.onIncomingDamageReceivers, this);
                    if (i > -1)
                        HG.ArrayUtils.ArrayRemoveAtAndResize(ref CharacterBody.healthComponent.onIncomingDamageReceivers, CharacterBody.healthComponent.onIncomingDamageReceivers.Length, i);
                }
            }
        }
    }
}