using R2API;
using RoR2;
using RoR2.Items;
using System;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    
    public sealed class X4 : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("X4");

        //[ConfigurableField(ConfigDesc = "Cooldown reduction per X-4 Stimulant. (1 = 1 second)")]
        //[TokenModifier("SS2_ITEM_X4_DESC", StatTypes.Default, 0)]
        public static float secCooldown = 0.25f;
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
                if (body.skillLocator.secondaryBonusStockSkill)
                {
                    float skillCD = body.skillLocator.secondaryBonusStockSkill.baseRechargeInterval; //base cooldown of relevant skill
                    float hyperbolicCDR = MSUtil.InverseHyperbolicScaling(secCooldown, secCooldown, skillCD - 1f, stack);
                    float cdr;
                    if(hyperbolicCDR > stack * secCooldown) //so hyperbolic is what i think we'd want here, but it vastly overpreforms with low stacks & and long cooldown. so i just take the lower value
                    {
                        cdr = stack * secCooldown;
                    }
                    else
                    {
                        cdr = hyperbolicCDR;
                    }
                    SS2Log.Debug("X4 RecalcStats end - " + hyperbolicCDR + " vs " + stack * secCooldown);
                    body.skillLocator.secondaryBonusStockSkill.flatCooldownReduction += cdr;
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
                if (skill == skill.characterBody.skillLocator.secondaryBonusStockSkill)
                {
                    float amntToHeal = flatHealth + self.healthComponent.health * (percentHealth + (percentHealthStacking * (stack - 1)));
                    self.healthComponent.Heal(amntToHeal, ignoredProcs, true);
                }
                //SS2Log.Debug("print list!: family: " + skill.skillFamily + " |  _family: " + skill._skillFamily + " |  " + skill.skillOverrides);
                //if(skill.skill
                //throw new NotImplementedException();
            }
        }
    }
}