using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class CoffeeBag : ItemBase
    {
        public const string token = "SS2_ITEM_COFFEEBAG_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("CoffeeBag");

        [ConfigurableField(ConfigDesc = "Duration before the buff expires, per stage, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float stageTimer = 60f;

        [ConfigurableField(ConfigDesc = "Attack speed bonus granted per stack at the start of the stage. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float atkSpeedBonus = .225f;

        [ConfigurableField(ConfigDesc = "Movement speed bonus granted per stack at the start of the stage. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float moveSpeedBonus = .21f;

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier //IStatItemBehavior
        {

            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.CoffeeBag;

            int count = 0;
            bool first = true;

            private void Awake()
            {
                base.Awake();

                float stacks; // = stageTimer / 2f;
                stacks = stageTimer;
                int buffCount = body.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag);
                stacks += buffCount;
                if (buffCount > 0)
                {
                    for (int i = 0; i < buffCount; i++)
                    {
                        body.RemoveOldestTimedBuff(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
                    }
                }
                for (int i = 0; i < stacks; i++)
                {
                    body.AddTimedBuff(SS2Content.Buffs.BuffCoffeeBag, i);
                }
                //count = 1;
            }

            public void OnEnable()
            {
                On.RoR2.CharacterBody.OnInventoryChanged += CoffeeBagInvChanged;
            }

            private void CoffeeBagInvChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
            {
                orig(self);
                //SS2Log.Debug("item collected");
                //int amnt = self.inventory.GetItemCount();
                if (self.hasAuthority && self.inventory)
                {
                    int amount = self.inventory.GetItemCount(SS2Content.Items.CoffeeBag.itemIndex);
                    //SS2Log.Debug("amount:" + amount + " | " + count);
                    if (amount > 0)
                    {
                        if (amount > count + 1)
                        {
                            int buffCount = self.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
                            int cycles = (int)stageTimer / 2;
                            if (buffCount > 0)
                            {
                                cycles += buffCount;
                                for (int i = 0; i < buffCount; i++)
                                {
                                    self.RemoveOldestTimedBuff(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
                                }
                            }
                            for (float i = 1; i <= cycles; i++)
                            {
                                self.AddTimedBuffAuthority(SS2Content.Buffs.BuffCoffeeBag.buffIndex, i);
                            }
                            //count = amount;
                        }
                        count = amount;
                    }
                    //count = amount;
                }
            }

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag) > 0)
                {
                    args.attackSpeedMultAdd += atkSpeedBonus * stack;
                    args.moveSpeedMultAdd += moveSpeedBonus * stack;
                }

            }

            private void OnDestroy()
            {
                if (body.HasBuff(SS2Content.Buffs.BuffCoffeeBag))
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffCoffeeBag);
                }
            }
        }
    }
}
