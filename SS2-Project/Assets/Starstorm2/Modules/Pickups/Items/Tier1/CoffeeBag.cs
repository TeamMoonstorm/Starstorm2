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

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier //IStatItemBehavior
        {

            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.CoffeeBag;

                        
            private void Awake()
            {
                base.Awake();
                for(int i = 0; i < stageTimer; i++)
                {
                    body.AddTimedBuff(SS2Content.Buffs.BuffCoffeeBag, i);
                }
            }

            
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if(body.GetBuffCount(SS2Content.Buffs.BuffCoffeeBag) > 0)
                {
                    args.attackSpeedMultAdd += atkSpeedBonus * stack;
                    args.moveSpeedMultAdd += moveSpeedBonus * stack;
                }

            }

            private void OnDestroy()
            {
                if(body.HasBuff(SS2Content.Buffs.BuffCoffeeBag))
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffCoffeeBag);
                }
            }
        }

    }
}
