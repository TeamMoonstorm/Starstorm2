using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Moonstorm.Starstorm2.Items
{
    [DisabledContent]
    public sealed class RelicOfEchelon : ItemBase
    {
        private const string token = "SS2_ITEM_RELICOFECHELON_DESC";

        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfEchelon", SS2Bundle.Items);

        [ConfigurableField(ConfigDesc = "Equipment cooldown increase per use, per stack.")]
        [TokenModifier(token, StatTypes.MultiplyByN, 0, "100")]
        public static float cooldownIncrease = .15f;

        [ConfigurableField(ConfigDesc = "Base amount of additional base damage added.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float damageBonus = 150;

        [ConfigurableField(ConfigDesc = "Base amount of additional base health added.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float healthBonus = 5000;

        [ConfigurableField(ConfigDesc = "Duration of the Echelon buff. Does not scale with stacks. (1 = 1 second)")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float buffDuration = 8;

        public override void Initialize()
        {
            base.Initialize();
            On.RoR2.Inventory.CalculateEquipmentCooldownScale += (orig, self) =>
            {
                //SS2Log.Debug("begin1");
                float num = orig.Invoke(self);
                //SS2Log.Debug("begin1.2");
                EquipmentIndex eqp = self.currentEquipmentIndex;
                var token = self.gameObject.GetComponent<EchelonToken>();
                var def = EquipmentCatalog.GetEquipmentDef(eqp);

                int useCount;
                int itemCount = self.GetItemCount(SS2Content.Items.RelicOfEchelon);
                if (itemCount > 0)
                {
                    //int multiplier;
                    if (token)
                    {
                        //token exists. this is not the first use with echelon.
                        //SS2Log.Debug("token" + token + " eqp: " + eqp);
                        if (token.usedEquipment.ContainsKey(eqp)) //token has the equipment that is currently being used
                        {
                            //int useCount = token.usedEquipment[eqp];

                            //token.usedEquipment[eqp] += itemCount;
                            //useCount = token.usedEquipment[eqp];
                            //int count = self.GetItemCount(SS2Content.Items.RelicOfEchelon);
                            //var def = EquipmentCatalog.GetEquipmentDef(eqp);
                            //multiplier = itemCount + useCount;
                            //float baseCD = def.cooldown;
                            //float adjusted = def.cooldown + (6.5f * useCount);
                            //float mod = adjusted / def.cooldown;

                            //goddamn it

                            //float cooldownAdjustment = 1 - ((1 + def.cooldown) / def.cooldown); //acounts for low cooldown equipments 
                            ////count = token.usedEquipment[eqp];
                            //float modifier = 1 + (.15f * Mathf.Pow(count, 2f)); // .1 + (Mathf.Pow(count, 2));
                            //float mod = Mathf.Pow(1.2f + cooldownAdjustment, count);
                            ////var val = (1 + MSUtil.InverseHyperbolicScaling(cooldownIncrease, cooldownIncrease, 0.7f, count));
                            //SS2Log.Debug("val: " + modifier + " | count: " + count + " | num: " + num + " | ans: " + num * modifier + " ans2: " + num * mod);
                            //num *= val;
                            //float adjusted = def.cooldown + (6.5f * useCount);
                            //float mod = adjusted / def.cooldown;
                            //num *= mod;
                        }
                        else
                        {
                            //Debug.Log("Test!");
                            //multiplier = itemCount;
                            //token.usedEquipment.Add(eqp, itemCount);
                            token.usedEquipment.Add(eqp, itemCount);
                        }
                    }
                    else
                    {
                        //SS2Log.Debug("No token? :pleading:");
                        //no token. so this is the first use, ever
                        //itemCount = self.GetItemCount(SS2Content.Items.RelicOfEchelon);
                        //multiplier = itemCount;
                        token = self.gameObject.AddComponent<EchelonToken>();
                        token.usedEquipment = new Dictionary<EquipmentIndex, int>();
                        token.usedEquipment.Add(eqp, itemCount);

                    }

                    //float adjusted = def.cooldown + (6.5f * multiplier);
                    //float mod = adjusted / def.cooldown;
                    //num *= mod;
                    useCount = token.usedEquipment[eqp];
                    //int count = self.GetItemCount(SS2Content.Items.RelicOfEchelon);
                    //var def = EquipmentCatalog.GetEquipmentDef(eqp);
                    //float cooldownAdjustment = Math.Abs(((.7f + def.cooldown) / def.cooldown) - 1);
                    float mod = Mathf.Pow(1 + cooldownIncrease, useCount);
                    float cooldownAdjustment = Math.Abs(((.7f + def.cooldown) / def.cooldown) - 1);
                    if (def.cooldown < 5) //really didnt want to do this but it normalizes the otherwise weird extra cd that exec card gets
                    {
                        cooldownAdjustment -= Mathf.Max(0, 6 - useCount);
                    }
                    float adjusted = def.cooldown + cooldownAdjustment;
                    float adj2 = adjusted * mod;
                    float mod2 = adj2 / def.cooldown;

                    //SS2Log.Debug("count: " + useCount + " | mod: " + mod + " | adj: " + adjusted + " | cdadj: " + cooldownAdjustment + " | mod2: " + mod2 + " | ans2: " + num * mod2);
                    num *= mod2;
                }
                return num;
            };
        }

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfEchelon;

            private void OnEnable()
            {
                EquipmentSlot.onServerEquipmentActivated += GrantEchelonBuff;
            }

            private void OnDisable()
            {
                EquipmentSlot.onServerEquipmentActivated -= GrantEchelonBuff;
            }

            private void GrantEchelonBuff(EquipmentSlot arg1, EquipmentIndex arg2)
            {
                //SS2Log.Debug("beginhook2");
                var token = arg1.characterBody.inventory.gameObject.GetComponent<EchelonToken>();
                var inv = arg1.characterBody.inventory;
                //int itemCount = self.GetItemCount(SS2Content.Items.RelicOfEchelon);
                if (inv)
                {
                    int count = inv.GetItemCount(SS2Content.Items.RelicOfEchelon);
                    if (count > 0)
                    {
                        if (!token)
                        {
                            SS2Log.Debug("Token added in OnEquipmentActivated - should probably never happen, but may be fine");
                            token = arg1.characterBody.inventory.gameObject.AddComponent<EchelonToken>();
                            token.usedEquipment = new Dictionary<EquipmentIndex, int>();
                        }
                        if (token.usedEquipment.ContainsKey(arg2))
                        {
                            token.usedEquipment[arg2] += count;

                        }
                        else
                        {
                            token.usedEquipment.Add(arg2, count);

                        }
                        arg1.characterBody.AddTimedBuffAuthority(SS2Content.Buffs.BuffEchelon.buffIndex, buffDuration);
                    }
                }
            }
            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (body.HasBuff(SS2Content.Buffs.BuffEchelon))
                {
                    //the base amounts are added by the buff itself in case the buff is gained from another source such as Aetherium's Accursed Potion
                    args.baseDamageAdd += (damageBonus * (stack - 1));
                    args.baseHealthAdd += (healthBonus * (stack - 1));
                }
            }
        }

        public class EchelonToken : MonoBehaviour
        {
            //helps keep track of the target and player responsible
            //public CharacterBody PlayerOwner;
            public IDictionary<EquipmentIndex, int> usedEquipment;

        }

    }
}
