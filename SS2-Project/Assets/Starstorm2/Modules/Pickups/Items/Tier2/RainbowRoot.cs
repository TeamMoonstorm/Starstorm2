using R2API;
using RoR2;
using RoR2.Items;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{

    public sealed class RainbowRoot : ItemBase
    {
        private const string token = "SS2_ITEM_RAINBOWROOT_DESC";

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Base portion of damage prevented to be gained as barrier. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 100)]
        public static float baseAmount = .25f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Portion of damage prevented to be gained as barrier per stack. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 100)]
        public static float scalingAmount = .15f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of armor gained upon pickup. Does not scale with stack count. (1 = 1 armor)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 100)]
        public static float baseArmor = 20;

        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RainbowRoot", SS2Bundle.Items);
        public override void Initialize()
        {
            On.RoR2.HealthComponent.TakeDamage += RootFunc;
        }

        private void RootFunc(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            orig(self, damageInfo);
            if (self.body)
            {
                if (self.body.inventory)
                {
                    var stackCount = self.body.GetItemCount(SS2Content.Items.RainbowRoot);

                    if (stackCount > 0)
                    {
                        float redu;
                        float armor = self.body.armor;
                        float plateRedu = 0;
                        if (damageInfo.rejected && self.body.isPlayerControlled)
                        {
                            redu = damageInfo.damage;
                        }
                        else
                        {
                            redu = armor / (armor + 100f);
                            redu *= damageInfo.damage;

                            int plate = self.itemCounts.armorPlate;
                            if (plate > 0)
                            {
                                float remainder = damageInfo.damage - redu;
                                plateRedu = remainder - Mathf.Max(1f, remainder - 5f * (float)plate);
                            }
                        }

                        float mult = MSUtil.InverseHyperbolicScaling(baseArmor, scalingAmount, 1, stackCount);
                        //SS2Log.Info("mult: " + mult + " | reduction: " + redu + " | plateredu: " + plateRedu + " | player hp: " + self.health + " | incoming damage: " + damageInfo.damage);
                        self.AddBarrierAuthority((redu + plateRedu) * mult);
                    }
                }
            }
        }

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RainbowRoot;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args){
                args.armorAdd += baseArmor;
            }

        }
    }
}
