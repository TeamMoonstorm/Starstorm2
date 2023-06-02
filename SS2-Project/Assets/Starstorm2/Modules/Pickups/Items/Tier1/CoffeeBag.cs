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
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("CoffeeBag", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of the buff gained upon using an interactable.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float interactBuff = 15;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Attack speed bonus granted per stack while the buff is active. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 1, "100")]
        public static float atkSpeedBonus = .225f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Movement speed bonus granted per stack while the buff is active. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float moveSpeedBonus = .21f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Max duration of buff, per stack. (1 = 1 second)")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float maxDurationStacking = 15;



        override public void Initialize()
        {
            On.RoR2.GlobalEventManager.OnInteractionBegin += CoffeeBagBuff;
            RecalculateStatsAPI.GetStatCoefficients += CalculateStatsCoffeeBag;
        }

        private void CalculateStatsCoffeeBag(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag) > 0)
            {
                int stack = sender.GetItemCount(SS2Content.Items.CoffeeBag);
                args.attackSpeedMultAdd += atkSpeedBonus;// * stack;
                args.moveSpeedMultAdd += moveSpeedBonus;// * stack;
            }
        }

        private void CoffeeBagBuff(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactableObject)
        {
            orig(self, interactor, interactable, interactableObject);
            var cb = interactor.GetComponent<CharacterBody>();
            if (cb)
            {
                if (cb.inventory)
                {
                    int count = cb.inventory.GetItemCount(SS2Content.Items.CoffeeBag);
                    if (count > 0)
                        if (SS2Util.CheckIsValidInteractable(interactable, interactableObject))
                        {
                            ApplyCoffeeBagBuff(cb);
                        }
                }
            }
        }

        private void ApplyCoffeeBagBuff(CharacterBody cb)
        {
            int buffCount = cb.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag);
            //if (buffCount > 0)
            //{
            //    for (int i = 0; i < buffCount; i++)
            //    {
            //        cb.RemoveOldestTimedBuff(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
            //    }
            //}
            //
            //for (int i = 1; i <= interactBuff + buffCount; i++)
            //{
            //    cb.AddTimedBuffAuthority(SS2Content.Buffs.BuffCoffeeBag.buffIndex, i);
            //}
            int j = 0;
            for(int i = buffCount; i < (maxDurationStacking * cb.GetItemCount(SS2Content.Items.CoffeeBag)); ++i)
            {
                ++j;
                cb.AddTimedBuffAuthority(SS2Content.Buffs.BuffCoffeeBag.buffIndex, i + 1);
                if(j >= 15) 
                {
                    //SS2Log.Info("added " + j + " stacks, ending");
                    break;

                }
            }
        }

        //public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier//IStatItemBehavior
        //{
        //
        //    [ItemDefAssociation]
        //    private static ItemDef GetItemDef() => SS2Content.Items.CoffeeBag;
        //
        //    //private void OnEnable()
        //    //{
        //    //    On.RoR2.GlobalEventManager.OnInteractionBegin += CoffeeBagBuff;
        //    //}
        //    //
        //    //private void OnDisable()
        //    //{
        //    //    On.RoR2.GlobalEventManager.OnInteractionBegin -= CoffeeBagBuff;
        //    //}
        //    //
        //    //private void CoffeeBagBuff(On.RoR2.GlobalEventManager.orig_OnInteractionBegin orig, GlobalEventManager self, Interactor interactor, IInteractable interactable, GameObject interactableObject)
        //    //{
        //    //    orig(self, interactor, interactable, interactableObject);
        //    //    var cb = interactor.GetComponent<CharacterBody>();
        //    //    if (cb)
        //    //    {
        //    //        if (cb.inventory)
        //    //        {
        //    //            int count = cb.inventory.GetItemCount(SS2Content.Items.CoffeeBag);
        //    //            if (count > 0)
        //    //            if (SS2Util.CheckIsValidInteractable(interactable, interactableObject))
        //    //            {
        //    //                ApplyCoffeeBagBuff(cb);
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //
        //    public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        //    {
        //        //int count = body.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag);
        //        if (body.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag) > 0)
        //        {
        //            args.attackSpeedMultAdd += atkSpeedBonus * stack;
        //            args.moveSpeedMultAdd += moveSpeedBonus * stack;
        //
        //        }
        //    }
        //
        //    //private void ApplyCoffeeBagBuff(CharacterBody cb)
        //    //{
        //    //    int buffCount = cb.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag);
        //    //    if (buffCount > 0)
        //    //    {
        //    //        for (int i = 0; i < buffCount; i++)
        //    //        {
        //    //            cb.RemoveOldestTimedBuff(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
        //    //        }
        //    //    }
        //    //
        //    //    for (int i = 1; i <= interactBuff + buffCount; i++)
        //    //    {
        //    //        cb.AddTimedBuffAuthority(SS2Content.Buffs.BuffCoffeeBag.buffIndex, i);
        //    //    }
        //    //}
        //
        //
        //    //int count = 0;
        //    //bool first = true;
        //    //
        //    //private void Awake()
        //    //{
        //    //    base.Awake();
        //    //
        //    //    float stacks; // = stageTimer / 2f;
        //    //    stacks = stageTimer;
        //    //    int buffCount = body.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag);
        //    //    stacks += buffCount;
        //    //    if (buffCount > 0)
        //    //    {
        //    //        for (int i = 0; i < buffCount; i++)
        //    //        {
        //    //            body.RemoveOldestTimedBuff(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
        //    //        }
        //    //    }
        //    //    for (int i = 0; i < stacks; i++)
        //    //    {
        //    //        body.AddTimedBuff(SS2Content.Buffs.BuffCoffeeBag, i);
        //    //    }
        //    //    //count = 1;
        //    //}
        //    //
        //    //public void OnEnable()
        //    //{
        //    //    On.RoR2.CharacterBody.OnInventoryChanged += CoffeeBagInvChanged;
        //    //}
        //    //
        //    //private void CoffeeBagInvChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        //    //{
        //    //    orig(self);
        //    //    //SS2Log.Debug("item collected");
        //    //    //int amnt = self.inventory.GetItemCount();
        //    //    if (self.hasAuthority && self.inventory)
        //    //    {
        //    //        int amount = self.inventory.GetItemCount(SS2Content.Items.CoffeeBag.itemIndex);
        //    //        //SS2Log.Debug("amount:" + amount + " | " + count);
        //    //        if (amount > 0)
        //    //        {
        //    //            if (amount > count + 1)
        //    //            {
        //    //                int buffCount = self.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
        //    //                int cycles = (int)stageTimer / 2;
        //    //                if (buffCount > 0)
        //    //                {
        //    //                    cycles += buffCount;
        //    //                    for (int i = 0; i < buffCount; i++)
        //    //                    {
        //    //                        self.RemoveOldestTimedBuff(SS2Content.Buffs.BuffCoffeeBag.buffIndex);
        //    //                    }
        //    //                }
        //    //                for (float i = 1; i <= cycles; i++)
        //    //                {
        //    //                    self.AddTimedBuffAuthority(SS2Content.Buffs.BuffCoffeeBag.buffIndex, i);
        //    //                }
        //    //                //count = amount;
        //    //            }
        //    //            count = amount;
        //    //        }
        //    //        //count = amount;
        //    //    }
        //    //}
        //    //
        //    //public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
        //    //{
        //    //    if (body.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag) > 0)
        //    //    {
        //    //        args.attackSpeedMultAdd += atkSpeedBonus * stack;
        //    //        args.moveSpeedMultAdd += moveSpeedBonus * stack;
        //    //    }
        //    //
        //    //}
        //    //
        //    //private void OnDestroy()
        //    //{
        //    //    if (body.HasBuff(SS2Content.Buffs.BuffCoffeeBag))
        //    //    {
        //    //        body.RemoveBuff(SS2Content.Buffs.BuffCoffeeBag);
        //    //    }
        //    //}
        //}
    }
}
