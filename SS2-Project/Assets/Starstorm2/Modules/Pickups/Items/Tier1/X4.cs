using R2API;
using RoR2;
using RoR2.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    //this is a fuckin warzone wtf

    public sealed class X4 : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("X4", SS2Bundle.Items);

        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Cooldown reduction per X-4 Stimulant. (1 = 1 second)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 0)]
        public static float secCooldown = 0.25f;
        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Cooldown reduction per X-4 Stimulant. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_X4_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float cdReduction = .1f;
        //
        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Percent healing upon activating a secondary skill. (1 = 1% of max health)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Percentage, 1)]
        public static float percentHealth = .01f;
        //
        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Percent healing per stack upon activating a secondary skill. (1 = 1% of max health)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Percentage, 2)]
        public static float percentHealthStacking = .005f;
        //
        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Flat healing upon activating a secondary skill. (1 = 1 health point)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 3)]
        public static float flatHealth = 10;

        //public static float atkSpeedBonus = .2f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Regen boost when using secondary skill. (1 = 1hp/s)")]
        [TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 1)]
        public static float baseRegenBoost = 2f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Regen boost per stack when using secondary skill. (1 = 1hp/s)")]
        [TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 2)]
        public static float stackRegenBoost = 1f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Regen duration when using secondary skill. (1 = 1s)")]
        [TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 3)]
        public static float regenDuration = 3f;
        public static float extraRegeneration = 0.2f;

        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Regen buff cap.")]
        //public static int buffCap = 5;


        public static ProcChainMask ignoredProcs;

        public static ItemDisplay itemDisplay;

        public static Dictionary<string, bool> specialSkillNames;

        override public void Initialize()
        {
            //On.RoR2.CharacterBody.OnSkillActivated += X4HealOnSkillActivation;
            InitalizeSpecialSkillsList(); // hi nebby :)
            //SceneManager.sceneLoaded += AddCode; kill!!!!!!!
        }

        public void InitalizeSpecialSkillsList()
        {
            specialSkillNames = new Dictionary<string, bool> //Name of skill, isIgnored
            {
                { "SS2_EXECUTIONER2_IONCHARGE_NAME", true },
                { "SS2_EXECUTIONER2_IONBURST_NAME", false },
                { "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME", false },
                { "RAILGUNNER_SECONDARY_NAME", true },
                { "RAILGUNNER_SNIPE_HEAVY_NAME", false },
                { "RAILGUNNER_SECONDARY_ALT_NAME", true},
                { "RAILGUNNER_SNIPE_LIGHT_NAME", false }
            };
            //SS2Log.Info("Made list");
        }
        
        public sealed class Behavior : BaseItemBodyBehavior, IStatItemBehavior
        {
            [ItemDefAssociation]    
            private static ItemDef GetItemDef() => SS2Content.Items.X4;

            public void Start()
            {
                body.onSkillActivatedAuthority += X4HealOnSkillActivation;
                //SS2Log.Info("hook added on " + body + " |");
                //SS2Log.Info("hook added on " + body.baseNameToken + " |");
            }

            private void X4HealOnSkillActivation(GenericSkill skill)
            {
               
                //if (!NetworkServer.active)
                //{
                //SS2Log.Info("onSkillActivatedAuthority called" + body + " | " + body.baseNameToken);
                //    return;
                //}
                
                var charbody = skill.characterBody;
                if (charbody)
                {
                    //SS2Log.Info("active called");
                    if (charbody.inventory)
                    {
                        int count = charbody.inventory.GetItemCount(SS2Content.Items.X4.itemIndex);
                        if (count > 0)
                        {
                            bool success = specialSkillNames.TryGetValue(skill.skillDef.skillNameToken, out bool isIgnored);
                            //bool possible = true;
                            //SS2Log.Info("skill bonus: " + skill.bonusStockFromBody + " | skill: " + skill.skillNameToken + " | recharg: " + skill.baseRechargeInterval + " | stok: " + skill.baseStock);
                            if (success)
                            {
                                //SS2Log.Info("found " + skill.skillDef.skillNameToken + " | " + isIgnored);
                                if (isIgnored)
                                {
                                    //SS2Log.Info("Function ENDS"); 
                                    return;
                                }

                            }
                            else
                            {
                                isIgnored = true;
                            }
                            if (!isIgnored || (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill && skill.baseRechargeInterval != 0)) // if it doesnt have a zero sec cooldown, max buffs at the number of bonus stocks
                            {
                                int buffcap = skill.bonusStockFromBody + 1;

                                if (!isIgnored)
                                {
                                    var secondary = skill.characterBody.skillLocator.secondaryBonusStockSkill;
                                    //SS2Log.Info("was valid!");
                                    if (skill.skillDef.skillNameToken == "SS2_EXECUTIONER2_IONBURST_NAME")
                                    {
                                        //var secondary = skill.characterBody.skillLocator.secondaryBonusStockSkill;
                                        if (secondary.skillNameToken == "SS2_EXECUTIONER2_IONCHARGE_NAME")
                                        {
                                            if (secondary.stock <= 0)
                                            {
                                                //SS2Log.Info("ion gun sucks");
                                                return;
                                            }

                                        }
                                        //else if (skill.baseRechargeInterval == 0)
                                        //{
                                        //    //SS2Log.Info("fuck railgunner");
                                        //    charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                        //    int buffCount1 = charbody.GetBuffCount(SS2Content.Buffs.BuffX4);
                                        //    if (buffCount1 != 0)
                                        //    {
                                        //        charbody.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
                                        //        //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                        //    }
                                        //}

                                    }

                                    if (skill.skillDef.skillNameToken != secondary.skillNameToken) //if skill is technically not secondary slot, 
                                    {
                                        buffcap = secondary.bonusStockFromBody + 1; //this assumes that the """SECONDARY""" being used should benefit from backup mags. for at least all base, dlc1, and ss2 characters, this is true
                                        //SS2Log.Info("Overrode stocks from " + skill.bonusStockFromBody + 1 + " to " + buffcap);
                                    }

                                }
                                //int buffcap = skill.bonusStockFromBody + 1;
                                //SS2Log.Debug(buffcap + " " + skill.maxStock + " " + skill.baseStock + " "+ skill.stock);
                                //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                int buffCount = charbody.GetBuffCount(SS2Content.Buffs.BuffX4);
                                //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                if (buffCount < buffcap)
                                {
                                    //SS2Log.Info("aghhhh 1 | " + buffCount + " | " + buffcap);
                                    charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                    //self.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
                                }
                                else if (buffCount == buffcap && buffCount >= 1)
                                {
                                    //SS2Log.Info("aghhhh 2 | " + buffCount + " | " + buffcap);

                                    //SS2Log.Info("removing stack " + charbody.GetBuffCount(SS2Content.Buffs.BuffX4.buffIndex));
                                    //charbody.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
                                    // SS2Log.Info("has removed STACK " + charbody.GetBuffCount(SS2Content.Buffs.BuffX4.buffIndex));

                                    //charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                    SS2Util.RefreshOldestBuffStack(charbody, SS2Content.Buffs.BuffX4, regenDuration);

                                }
                                //else if(buffCount == buffcap && buffCount == 1)
                                //{
                                //    //SS2Log.Info("aghhhh 3 | " + buffCount + " | " + buffcap);
                                //    //charbody.RemoveBuff(SS2Content.Buffs.BuffX4.buffIndex);
                                //    //charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration); if i do this i get -1 stacks sometimes im so pissed
                                //    for (int i = 0; i < body.timedBuffs.Count; i++)
                                //    {
                                //        if (body.timedBuffs[i].buffIndex == SS2Content.Buffs.BuffX4.buffIndex)
                                //        {
                                //            body.timedBuffs[i].timer = regenDuration;
                                //            break; //this is a little shit but it's probably fine right
                                //        }
                                //    }
                                //
                                //}


                                //SS2Log.Info("bonus: " + skill.bonusStockFromBody + " | cap: " + buffcap + " | buffcount: " + buffCount + " | final amount: " + charbody.GetBuffCount(SS2Content.Buffs.BuffX4));
                                //if (buffCount < maxBuffStack)
                                //{
                                //    body.AddTimedBuffAuthority(SS2Content.Buffs.BuffKickflip.buffIndex, buffDuration); //i swear if this works im killing hopoo
                                //}
                                //else if (buffCount == maxBuffStack)
                                //{
                                //    body.RemoveOldestTimedBuff(SS2Content.Buffs.BuffKickflip.buffIndex);
                                //    body.AddTimedBuffAuthority(SS2Content.Buffs.BuffKickflip.buffIndex, buffDuration); //this is what skateboard does
                                //}
                                //else
                                //{
                                //    self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration); //why am i like this
                                //}
                                //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);

                            }
                            else if (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill) // if the skill is spammable, cap the buff count at 1
                            {
                                //SS2Log.Info("pee shit explosion!!!");
                                charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                int buffCount = charbody.GetBuffCount(SS2Content.Buffs.BuffX4);
                                if (buffCount != 0)
                                {
                                    charbody.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
                                    //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                }
                                //else
                                //{
                                //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                //}

                            }


                        }
                    }
                }

                //if (self.inventory)
                //{
                //    //SS2Log.Debug("skill" + skill.skillDef.skillNameToken);
                //    int count = self.inventory.GetItemCount(SS2Content.Items.X4.itemIndex); //again i could use stack here probably but i just wanna make sure this works we can fix it later
                //                                                                            //SS2Log.Info("count: " + count + " | base skill name token: " + skill.baseSkill.skillNameToken + " | skill: " + skill.defaultSkillDef + " | skill: " + skill.name + " |||| " + skill.characterBody.skillLocator.secondaryBonusStockSkill + " | " + skill.characterBody.skillLocator.secondary);
                //    if (count > 0)
                //    {//|| skill.baseSkill.skillNameToken == SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME
                //        if (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill && skill.baseRechargeInterval != 0 || skill.baseSkill.skillNameToken == "SS2_NEMCOMMANDO_SECONDARY_SHOOT_NAME") // if it doesnt have a zero sec cooldown, max buffs at the number of bonus stocks
                //        {
                //
                //            int buffcap = skill.bonusStockFromBody + 1;
                //            //SS2Log.Debug(buffcap + " " + skill.maxStock + " " + skill.baseStock + " "+ skill.stock);
                //            //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                //            int buffCount = self.GetBuffCount(SS2Content.Buffs.BuffX4);
                //            //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                //            if (buffCount < buffcap)
                //            {
                //                SS2Log.Info("aghhhh 1 | " + buffCount + " | " + buffcap);
                //                self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                //                //self.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
                //            }
                //            else if (buffCount == buffcap)
                //            {
                //                SS2Log.Info("aghhhh 2 | " + buffCount + " | " + buffcap);
                //                self.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4);
                //                self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                //            }
                //
                //            SS2Log.Info("bonus: " + skill.bonusStockFromBody + " | cap: " + buffcap + " | buffcount: " + buffCount + " | final amount: " + self.GetBuffCount(SS2Content.Buffs.BuffX4));
                //            //if (buffCount < maxBuffStack)
                //            //{
                //            //    body.AddTimedBuffAuthority(SS2Content.Buffs.BuffKickflip.buffIndex, buffDuration); //i swear if this works im killing hopoo
                //            //}
                //            //else if (buffCount == maxBuffStack)
                //            //{
                //            //    body.RemoveOldestTimedBuff(SS2Content.Buffs.BuffKickflip.buffIndex);
                //            //    body.AddTimedBuffAuthority(SS2Content.Buffs.BuffKickflip.buffIndex, buffDuration); //this is what skateboard does
                //            //}
                //            //else
                //            //{
                //            //    self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration); //why am i like this
                //            //}
                //            //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                //
                //        }
                //        else if (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill) // if the skill is spammable, cap the buff count at 1
                //        {
                //            SS2Log.Info("pee shit explosion!!!");
                //            self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                //            int buffCount = self.GetBuffCount(SS2Content.Buffs.BuffX4);
                //            if (buffCount != 0)
                //            {
                //                self.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
                //                //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                //            }
                //            //else
                //            //{
                //            //self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                //            //}
                //
                //        }
                //    }
                //
                //}
            }

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

            public void RecalculateStatsStart() { }

            private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args) { }

            //private void X4AtkSpeedOnSkill(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
            //{
            //    SS2Log.Debug("pissing");
            //    orig(self, skill);
            //    if (self.inventory)
            //    {
            //        SS2Log.Debug("hell yea");
            //        int count = self.inventory.GetItemCount(SS2Content.Items.X4.itemIndex);
            //        if(count > 0 && skill == skill.characterBody.skillLocator.secondaryBonusStockSkill)
            //        {
            //            self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, atkSpeedDuration);
            //            SS2Log.Debug("explosin: " + skill.rechargeStopwatch + " | base: " + skill.baseRechargeInterval + " | base + base: " + skill.baseSkill.baseRechargeInterval);
            //            
            //            
            //            //if (self.GetBuffCount(SS2Content.Buffs.BuffX4) < buffCap)
            //            //{
            //            //    self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, atkSpeedDuration);
            //            //}
            //            //else if(self.GetBuffCount(SS2Content.Buffs.BuffX4) == buffCap)
            //            //{
            //            //    self.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
            //            //    self.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, atkSpeedDuration);
            //            //}
            //        }
            //    }
            //}

            

            //private void FixedUpdate()
            //{
            //    var skill = body.skillLocator.secondaryBonusStockSkill;
            //    var token = body.gameObject.GetComponent<X4Token>();
            //    if (!token)
            //    {
            //        token = body.gameObject.AddComponent<X4Token>();
            //        token.pastStock = skill.stock;
            //        token.usesLeft = 1 + skill.bonusStockFromBody;
            //    }
            //    else
            //    {
            //        int currentStock = skill.stock;
            //        if (currentStock > token.pastStock)
            //        {
            //            token.usesLeft++;
            //            if (token.usesLeft > 1 + skill.bonusStockFromBody)
            //            {
            //                token.usesLeft = 1 + skill.bonusStockFromBody;
            //            }
            //            //token.pastStock++;
            //        }
            //
            //
            //        token.pastStock = skill.stock;
            //    }
            //}

        }
        //public class X4Token : MonoBehaviour
        //{
        //    public int pastStock;
        //    public int usesLeft;
        //    //helps keep track of the target and player responsible
        //    public CharacterBody PlayerOwner;
        //}
    }
}