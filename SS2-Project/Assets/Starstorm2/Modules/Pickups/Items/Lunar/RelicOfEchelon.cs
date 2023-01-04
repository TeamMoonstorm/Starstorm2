using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class RelicOfEchelon : ItemBase
    {
        private const string token = "SS2_ITEM_RELICOFECHELON_DESC";

        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfEchelon");

        [ConfigurableField(ConfigDesc = "Time between marks in seconds.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float cooldownIncrease = .2f;

        public sealed class Behavior : BaseItemBodyBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfEchelon;

            public new void Init()
            {
                On.RoR2.Inventory.CalculateEquipmentCooldownScale += (orig, self) =>
                {
                    float num = orig.Invoke(self);
                    EquipmentIndex eqp = self.currentEquipmentIndex;
                    var token = self.gameObject.GetComponent<EchelonToken>();
                    int count;
                    if (token.usedEquipment.ContainsKey(eqp)) // i don't think this should ever fail but maybe it will so uh this
                    {
                        count = token.usedEquipment[eqp];
                        num *= (1 + MSUtil.InverseHyperbolicScaling(cooldownIncrease, cooldownIncrease, 0.7f, count));
                    }
                    else
                    {
                        //count = 0;
                    }
                    //num *= MSUtil.InverseHyperbolicScaling(cooldownIncrease, cooldownIncrease, 0.7f, count);
                    return num;
                };
            }

            private void OnEnable()
            {
                EquipmentSlot.onServerEquipmentActivated += GrantEchelonBuff;
            }

            private void OnDisable()
            {
                EquipmentSlot.onServerEquipmentActivated += GrantEchelonBuff;
            }

            private void GrantEchelonBuff(EquipmentSlot arg1, EquipmentIndex arg2)
            {
                var token = arg1.characterBody.inventory.gameObject.GetComponent<EchelonToken>();
                
                if (!token)
                {
                    token = arg1.characterBody.inventory.gameObject.AddComponent<EchelonToken>(); 
                }
                if (token.usedEquipment.ContainsKey(arg2))
                {
                    token.usedEquipment[arg2]++;

                }
                else
                {
                    token.usedEquipment.Add(arg2, 1);

                }
                int val = token.usedEquipment[arg2];
                SS2Log.Debug("cd1: " + arg1.subcooldownTimer);
                

            }
            //public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            //{
            //    args.baseHealthAdd += (body.baseMaxHealth + (body.levelMaxHealth * (body.level - 1))) * stack;
            //    //args.moveSpeedMultAdd += stack / 2;
            //}
            //public void RecalculateStatsStart()
            //{
            //
            //}
            //
            //public void RecalculateStatsEnd()
            //{
            //    body.acceleration = body.baseAcceleration / (stack * acclMult);
            //}
        }

        public class EchelonToken : MonoBehaviour
        {
            //helps keep track of the target and player responsible
            //public CharacterBody PlayerOwner;
            public IDictionary<EquipmentIndex, int> usedEquipment;

        }

    }
}
