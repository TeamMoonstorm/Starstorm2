using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]

    public sealed class X4 : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("X4");

        //[ConfigurableField(ConfigDesc = "Cooldown reduction per X-4 Stimulant. (1 = 1 second)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 0)]
        public static float secCooldown = 0.25f;
        public static float cdReduction = .1f;
        //
        //[ConfigurableField(ConfigDesc = "Percent healing upon activating a secondary skill. (1 = 1% of max health)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Percentage, 1)]
        public static float percentHealth = .01f;
        //
        //[ConfigurableField(ConfigDesc = "Percent healing per stack upon activating a secondary skill. (1 = 1% of max health)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Percentage, 2)]
        public static float percentHealthStacking = .005f;
        //
        //[ConfigurableField(ConfigDesc = "Flat healing upon activating a secondary skill. (1 = 1 health point)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 3)]
        public static float flatHealth = 10;

        public static ProcChainMask ignoredProcs;

        public sealed class Behavior : BaseItemBodyBehavior, IStatItemBehavior
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.X4;
            public void RecalculateStatsEnd()
            {
                if (body.skillLocator)
                {
                    if (body.skillLocator.secondaryBonusStockSkill)
                    {
                        float skillCD = body.skillLocator.secondaryBonusStockSkill.baseRechargeInterval; //base cooldown of relevant skill
                        //float hyperbolicCDR = MSUtil.InverseHyperbolicScaling(secCooldown, secCooldown, skillCD - 1f, stack);
                        //float cdr;
                        //if (hyperbolicCDR > stack * secCooldown) //so hyperbolic is what i think we'd want here, but it vastly overpreforms with low stacks & and long cooldown. so i just take the lower value
                        //{
                        //    cdr = stack * secCooldown;
                        //}
                        //else
                        //{
                        //    cdr = hyperbolicCDR;
                        //}
                        float timeMult = Mathf.Pow(1 - cdReduction, stack - 1);
                        //cdr = skillCD * timeMult;
                        //SS2Log.Debug("X4 RecalcStats - " + hyperbolicCDR + " vs " + stack * secCooldown + " vs " + cdr + "coldown scale: " + body.skillLocator.secondaryBonusStockSkill.cooldownScale);
                        body.skillLocator.secondaryBonusStockSkill.cooldownScale *= timeMult;
                        //SS2Log.Debug("X4 RecalcStats end " + body.skillLocator.secondaryBonusStockSkill.cooldownScale);
                    }
                }
                //args.cooldownMultAdd += (float)itemCount * Synergies.pearlCD.Value;
                //args.secondaryCooldownMultAdd *= MSUtil.InverseHyperbolicScaling(secCooldown, secCooldown, 0.7f, stack);
            }
            public void RecalculateStatsStart()
            {
            }

            //RecalculateStatsAPI.GetStatCoefficients += new RecalculateStatsAPI.StatHookEventHandler(Hook.RecalculateStatsAPI_GetStatCoefficients);
            //public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            //{
            //    //float cdr = MSUtil.InverseHyperbolicScaling(secCooldown, secCooldown, 0.7f, stack);
            //    //SS2Log.Debug("RecalcStats middle? - " + cdr);
            //    //args.cooldownReductionAdd += stack * secCooldown;
            //    
            //}

            private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
            {
            }
            private void OnEnable()
            {
                On.RoR2.CharacterBody.OnSkillActivated += X4HealOnSkillActivation;
            }

            private void OnDisable()
            {
                On.RoR2.CharacterBody.OnSkillActivated -= X4HealOnSkillActivation;
            }

            private void X4HealOnSkillActivation(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
            {
                orig(self, skill);
                //skill == skill.characterBody.skillLocator.secondaryBonusStockSkill
                //skill.skillFamily.ToString().Contains("Secondary")
                var token = body.gameObject.GetComponent<X4Token>();
                if (self.inventory && token)
                {
                    SS2Log.Debug("skill" + skill.skillDef.skillNameToken);
                    int count = self.inventory.GetItemCount(SS2Content.Items.X4.itemIndex); //again i could use stack here probably but i just wanna make sure this works we can fix it later
                    if (self.inventory.GetItemCount(DLC1Content.Items.ConvertCritChanceToCritDamage) > 0 && skill.skillDef.skillNameToken == "RAILGUNNER_SNIPE_HEAVY_NAME")
                    {
                        float amntToHeal = flatHealth + self.healthComponent.health * (percentHealth + (percentHealthStacking * (count - 1)));
                        self.healthComponent.Heal(amntToHeal, ignoredProcs, true);
                    }
                    else if (skill.skillDef.skillNameToken != "SNIPERCLASSIC_SECONDARY_NAME" && (skill.skillDef.skillNameToken == "SNIPERCLASSIC_PRIMARY_NAME" || skill.skillDef.skillNameToken == "SNIPERCLASSIC_PRIMARY_ALT_NAME" || skill.skillDef.skillNameToken == "SNIPERCLASSIC_PRIMARY_ALT2_NAME")) //sniper classic exceptions
                    {
                        float amntToHeal = flatHealth + self.healthComponent.health * (percentHealth + (percentHealthStacking * (count - 1)));
                        self.healthComponent.Heal(amntToHeal, ignoredProcs, true);
                    }
                    else if (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill && skill.skillDef.skillNameToken != "RAILGUNNER_SECONDARY_NAME" && count > 0)
                    {
                        if (skill.skillDef.skillNameToken == "SS2_EXECUTIONER_IONGUN_NAME" || skill.skillDef.skillNameToken == "RAILGUNNER_SECONDARY_ALT_NAME") //this is jank but it works :)
                        {
                            float amntToHeal = flatHealth + self.healthComponent.health * (percentHealth + (percentHealthStacking * (count - 1)));
                            self.healthComponent.Heal(amntToHeal, ignoredProcs, true);
                        }
                        else if (token.usesLeft > 0)
                        {
                            float amntToHeal = flatHealth + self.healthComponent.health * (percentHealth + (percentHealthStacking * (count - 1)));
                            self.healthComponent.Heal(amntToHeal, ignoredProcs, true);
                            token.usesLeft--;
                            SS2Log.Debug("skill cooldown:" + skill.CalculateFinalRechargeInterval());
                        }
                    }

                }
            }

            private void FixedUpdate()
            {
                var skill = body.skillLocator.secondaryBonusStockSkill;
                var token = body.gameObject.GetComponent<X4Token>();
                if (!token)
                {
                    token = body.gameObject.AddComponent<X4Token>();
                    token.pastStock = skill.stock;
                    token.usesLeft = 1 + skill.bonusStockFromBody;
                }
                else
                {
                    int currentStock = skill.stock;
                    if (currentStock > token.pastStock)
                    {
                        token.usesLeft++;
                        if (token.usesLeft > 1 + skill.bonusStockFromBody)
                        {
                            token.usesLeft = 1 + skill.bonusStockFromBody;
                        }
                        //token.pastStock++;
                    }


                    token.pastStock = skill.stock;
                }
            }

        }
        public class X4Token : MonoBehaviour
        {
            public int pastStock;
            public int usesLeft;
            //helps keep track of the target and player responsible
            public CharacterBody PlayerOwner;
        }
    }
}