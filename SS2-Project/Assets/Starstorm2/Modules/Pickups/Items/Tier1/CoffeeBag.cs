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
    }
}
