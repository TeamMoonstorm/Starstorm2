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
    //this is a fuckin warzone wtf - not anymore!!! z

    public sealed class X4 : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("X4", SS2Bundle.Items);

        public static float secCooldown = 0.25f;
        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Cooldown reduction per X-4 Stimulant. (1 = 100%)")]
        [TokenModifier("SS2_ITEM_X4_DESC", StatTypes.MultiplyByN, 0, "100")]
        public static float cdReduction = .1f;

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

        public static ProcChainMask ignoredProcs;

        public static ItemDisplay itemDisplay;

        public static Dictionary<string, bool> specialSkillNames;

        override public void Initialize()
        {
            InitalizeSpecialSkillsList(); // hi nebby :)
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
        }
        
        public sealed class Behavior : BaseItemBodyBehavior, IStatItemBehavior
        {
            [ItemDefAssociation]    
            private static ItemDef GetItemDef() => SS2Content.Items.X4;

            public void Start()
            {
                body.onSkillActivatedAuthority += X4HealOnSkillActivation;
            }

            private void X4HealOnSkillActivation(GenericSkill skill)
            {
                var charbody = skill.characterBody;
                if (charbody)
                {
                    if (charbody.inventory)
                    {
                        int count = charbody.inventory.GetItemCount(SS2Content.Items.X4.itemIndex);
                        if (count > 0)
                        {
                            bool success = specialSkillNames.TryGetValue(skill.skillDef.skillNameToken, out bool isIgnored);
                            if (success)
                            {
                                if (isIgnored)
                                {
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
                                    if (skill.skillDef.skillNameToken == "SS2_EXECUTIONER2_IONBURST_NAME")
                                    {
                                        if (secondary.skillNameToken == "SS2_EXECUTIONER2_IONCHARGE_NAME")
                                        {
                                            if (secondary.stock <= 0)
                                            {
                                                return;
                                            }
                                        }
                                    }

                                    if (skill.skillDef.skillNameToken != secondary.skillNameToken) //if skill is technically not secondary slot, 
                                    {
                                        buffcap = secondary.bonusStockFromBody + 1; //this assumes that the """SECONDARY""" being used should benefit from backup mags. for at least all base, dlc1, and ss2 characters, this is true
                                    }

                                }

                                int buffCount = charbody.GetBuffCount(SS2Content.Buffs.BuffX4);

                                if (buffCount < buffcap)
                                {
                                    charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);

                                }
                                else if (buffCount == buffcap && buffCount >= 1)
                                {
                                    SS2Util.RefreshOldestBuffStack(charbody, SS2Content.Buffs.BuffX4, regenDuration);
                                }
                            }
                            else if (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill) // if the skill is spammable, cap the buff count at 1
                            {
                                charbody.AddTimedBuffAuthority(SS2Content.Buffs.BuffX4.buffIndex, regenDuration);
                                int buffCount = charbody.GetBuffCount(SS2Content.Buffs.BuffX4);
                                if (buffCount != 0)
                                {
                                    charbody.RemoveOldestTimedBuff(SS2Content.Buffs.BuffX4.buffIndex);
                                }
                            }
                        }
                    }
                }
            }

            public void RecalculateStatsEnd()
            {
                if (body.skillLocator)
                {
                    if (body.skillLocator.secondaryBonusStockSkill)
                    {
                        //float skillCD = body.skillLocator.secondaryBonusStockSkill.baseRechargeInterval; //base cooldown of relevant skill

                        float timeMult = Mathf.Pow(1 - cdReduction, stack);
                        body.skillLocator.secondaryBonusStockSkill.cooldownScale *= timeMult;
                    }
                }
            }

            public void RecalculateStatsStart() { }

            private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args) { }
        }
    }
}