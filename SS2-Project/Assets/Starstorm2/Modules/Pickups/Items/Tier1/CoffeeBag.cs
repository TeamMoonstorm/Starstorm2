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
        [TokenModifier(token, StatTypes.Percentage, 1)]
        public static float atkSpeedBonus = .225f;

        [ConfigurableField(ConfigDesc = "Movement speed bonus granted per stack at the start of the stage. (1 = 100%)")]
        [TokenModifier(token, StatTypes.Percentage, 2)]
        public static float moveSpeedBonus = .21f;

        int count = 0;

        public override void Initialize()
        {
            RoR2.SceneDirector.onPrePopulateSceneServer += ResetCoffeeBag;
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
                SS2Log.Debug("amount:" + amount + " | " + count);
                if (amount > 0)
                {
                    if(amount > count)
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
                        count = amount;
                    }

                }
                //count = amount;
            }
        }

        private void ResetCoffeeBag(SceneDirector obj)
        {
            //SS2Log.Debug("director moment");
            foreach (var player in PlayerCharacterMasterController.instances)
            {
                if (player.body.inventory.GetItemCount(SS2Content.Items.CoffeeBag.itemIndex) > 0)
                {
                    for (float i = 1; i <= stageTimer; i++)
                    {
                        player.body.AddTimedBuffAuthority(SS2Content.Buffs.BuffCoffeeBag.buffIndex, i);
                    }
                }
            }

        }


        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier //IStatItemBehavior
        {
            /* I've decided to leave this part here as an example on how to move from the IStatItemBehavior to the IBodyStatArgModifier interface.
            /*public void RecalculateStatsStart()
            { }

            public void RecalculateStatsEnd()
            {
                body.attackSpeed += (body.baseAttackSpeed + body.levelAttackSpeed * (body.level - 1)) * atkSpeedBonus * stack;
                body.moveSpeed += (body.baseMoveSpeed + body.levelMoveSpeed * (body.level - 1)) * moveSpeedBonus * stack;
            }*/

            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.CoffeeBag;

            

            //public void Start()
            //{
            //    //var cb = GetComponent<CharacterBody>();
            //    //cb.gameObject.AddComponent<CoffeeToken>();
            //}
            

            //private void OnEnable()
            //{
            //    RoR2.SceneDirector.onPrePopulateSceneServer += ResetCoffeeBag;
            //    On.RoR2.CharacterBody.OnInventoryChanged += CoffeeBagInvChanged;
            //    SS2Log.Debug("enable");
            //
            //}
            //
            //private void OnDisable()
            //{
            //    RoR2.SceneDirector.onPrePopulateSceneServer -= ResetCoffeeBag;
            //    On.RoR2.CharacterBody.OnInventoryChanged -= CoffeeBagInvChanged;
            //}
            //
            //private void CoffeeBagInvChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
            //{
            //    orig(self);
            //    SS2Log.Debug("item collected");
            //    //int amnt = self.inventory.GetItemCount();
            //    if (self.hasAuthority && self.inventory)
            //    {
            //        int amount = self.inventory.GetItemCount(GetItemDef());
            //        SS2Log.Debug("amount:" + amount + " | " + count);
            //        if(amount > count)
            //        {
            //            int buffCount = self.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
            //            int cycles = (int)stageTimer / 2;
            //            if (buffCount > 0)
            //            {
            //                cycles += buffCount;
            //                for(int i = 0; i < buffCount; i++)
            //                {
            //                    self.RemoveOldestTimedBuff(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
            //                }
            //            }
            //            for (float i = 1; i <= cycles; i++)
            //            {
            //                self.AddTimedBuffAuthority(SS2Content.Buffs.BuffCoffeeBag.buffIndex, i);
            //            }
            //        }
            //        count = amount;
            //    }
            //}
            //
            //private void ResetCoffeeBag(SceneDirector obj)
            //{
            //    //SS2Log.Debug("director moment");
            //    foreach (var player in PlayerCharacterMasterController.instances)
            //    {
            //        if (player.body.inventory.GetItemCount(GetItemDef()) > 0)
            //        {
            //            for (float i = 1; i <= stageTimer; i++)
            //            {
            //                player.body.AddTimedBuffAuthority(SS2Content.Buffs.BuffCoffeeBag.buffIndex, i);
            //            }
            //        }
            //    }
            //
            //}

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //var token = body.GetComponent<CoffeeToken>();
                if(body.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag) > 0)
                {
                    SS2Log.Debug("has buff");
                    args.attackSpeedMultAdd += atkSpeedBonus * stack;
                    args.moveSpeedMultAdd += moveSpeedBonus * stack;
                }

            }
        }

    }
}
