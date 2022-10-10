using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using RoR2;
using RoR2.Items;
using System;
using UnityEngine;

namespace Moonstorm.Starstorm2.Items
{
    //[DisabledContent]
    public sealed class RelicOfForce : ItemBase
    {
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("RelicOfForce");

        [ConfigurableField(ConfigDesc = "Damage multipler per stack. (1 = 100% more damage, multiplicatively)")]
        [TokenModifier("SS2_ITEM_RELICOFFORCE_DESC", StatTypes.Percentage, 0)]
        public static float damageMultiplier = 1;

        [ConfigurableField(ConfigDesc = "Attack speed reduction and cooldown increase per stack. (1 = 100% slower attack speed and longer cooldowns)")]
        [TokenModifier("SS2_ITEM_RELICOFFORCE_DESC", StatTypes.Percentage, 1)]
        public static float forcePenalty = .4f;


        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfForce;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                args.damageMultAdd += damageMultiplier;
                float penalty = MSUtil.InverseHyperbolicScaling(forcePenalty, forcePenalty, 0.9f, stack);
                args.attackSpeedMultAdd -= penalty;
                //SS2Log.Debug("Relic Stats primarycd: " + args.primaryCooldownMultAdd);
                args.primaryCooldownMultAdd += penalty;
                args.secondaryCooldownMultAdd += penalty;
                args.utilityCooldownMultAdd += penalty;
                args.specialCooldownMultAdd += penalty;
                //SS2Log.Debug("Relic Stats primarycd post: " + args.primaryCooldownMultAdd);
            }

            private void OnEnable()
            {
                //On.RoR2.CharacterBody.OnSkillActivated += RelicOfForceIncreaseCooldown;
                //On.RoR2.CharacterBody.OnInventoryChanged += AddRelicCompontent;
                IL.RoR2.GenericSkill.CalculateFinalRechargeInterval += ForceSkillFinalRecharge;
            }

            private void OnDisable()
            {
                //On.RoR2.CharacterBody.OnSkillActivated -= RelicOfForceIncreaseCooldown;
                //On.RoR2.CharacterBody.OnInventoryChanged -= AddRelicCompontent;
                IL.RoR2.GenericSkill.CalculateFinalRechargeInterval -= ForceSkillFinalRecharge;
            }

            private void ForceSkillFinalRecharge(ILContext il)
            {
                ILCursor c = new ILCursor(il);
                if(c.TryGotoNext(
                    x => x.MatchLdarg(0),
                    x => x.MatchCallOrCallvirt<RoR2.GenericSkill>("get_baseRechargeInterval")
                    ))
                {
                    c.Remove();
                    //c.Index += 1;
                    c.Remove();
                }
                else
                {
                    SS2Log.Error("Failed to apply Relic of Force First IL Hook");
                }

                if (c.TryGotoNext(
                    x => x.MatchCallOrCallvirt<UnityEngine.Mathf>("Min")
                    ))
                {
                    c.Remove();

                    //c.EmitDelegate<Func<float, float, float>>((v1, v2) => v2);
                    //c.Emit(OpCodes.Ret);
                }
                else
                {
                    SS2Log.Error("Failed to apply Relic of Force IL Second Hook");
                }
            }

            //private void AddRelicCompontent(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
            //{
            //    orig(self);
            //    var forceToken = self.gameObject.GetComponent<ForceToken>();
            //    if (!forceToken)
            //    {
            //        self.gameObject.AddComponent<ForceToken>();
            //        forceToken = self.gameObject.GetComponent<ForceToken>();
            //        SS2Log.Debug("i love coding: " + forceToken + " | number:" + forceToken.hilariousValue);
            //        //forceToken.basePrimaryCD = self.skillLocator.primary.baseRechargeInterval;
            //        //forceToken.baseSecondaryCD = self.skillLocator.secondary.baseRechargeInterval;
            //        //forceToken.baseUtilityCD = self.skillLocator.utility.baseRechargeInterval;
            //        //forceToken.baseSpecialCD = self.skillLocator.special.baseRechargeInterval;
            //    }
            //
            //    //IL.RoR2.GenericSkill.CalculateFinalRechargeInterval += (il) =>
            //    //{
            //    //    ILCursor c = new ILCursor(il);
            //    //    if (c.TryGotoNext(
            //    //        x => x.MatchCallOrCallvirt<UnityEngine.Mathf>("Min")
            //    //        ))
            //    //
            //    //    {
            //    //        //c.Index += 12;
            //    //        c.Emit(OpCodes.Ret);
            //    //    }
            //    //    else
            //    //    {
            //    //        SS2Log.Debug("Failed to apply Relic of Force IL Hook");
            //    //    }
            //    //};
            //}

            //private void RelicOfForceIncreaseCooldown(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
            //{
            //    orig(self, skill);
            //    SS2Log.Debug("beginning hook");
            //    if (self.inventory)
            //    {
            //        if (stack > 0)
            //        {
            //            SS2Log.Debug("sufficient stacks ");
            //            float cooldownPenalty = MSUtil.InverseHyperbolicScaling(forcePenalty, forcePenalty, 0.9f, stack);
            //            var forceToken = self.gameObject.GetComponent<ForceToken>();
            //            if (skill == skill.characterBody.skillLocator.primary)
            //            {
            //                SS2Log.Debug("found primary");
            //                if (forceToken)
            //                {
            //                    SS2Log.Debug("found force token");
            //                    if (forceToken.primaryInc < stack)
            //                    {
            //                        float jankCD = body.skillLocator.primary.baseRechargeInterval * cooldownPenalty;
            //                        self.skillLocator.primary.finalRechargeInterval += jankCD;
            //                        forceToken.primaryInc = stack;
            //                        SS2Log.Debug("finished prmr: " + forceToken.primaryInc + " | jank: " + jankCD);
            //                    }
            //                }
            //            }
            //
            //            if (skill == skill.characterBody.skillLocator.secondary)
            //            {
            //                SS2Log.Debug("found secondary");
            //                if (forceToken)
            //                {
            //                    SS2Log.Debug("found force token");
            //                    if (forceToken.secondaryInc < stack)
            //                    {
            //                        float jankCD = body.skillLocator.secondary.baseRechargeInterval * cooldownPenalty;
            //                        self.skillLocator.secondary.finalRechargeInterval += jankCD;
            //                        forceToken.secondaryInc = stack;
            //                        SS2Log.Debug("finished secnd: " + forceToken.secondaryInc + " | jank: " + jankCD);
            //                    }
            //                }
            //            }
            //            if (skill == skill.characterBody.skillLocator.utility)
            //            {
            //                SS2Log.Debug("found utility");
            //                if (forceToken)
            //                {
            //                    SS2Log.Debug("found force token");
            //                    if (forceToken.utilityInc < stack)
            //                    {
            //                        float jankCD = body.skillLocator.utility.baseRechargeInterval * cooldownPenalty;
            //                        self.skillLocator.utility.finalRechargeInterval += jankCD;
            //                        forceToken.utilityInc = stack;
            //                        SS2Log.Debug("finished utilmty: " + forceToken.utilityInc + " | jank: " + jankCD);
            //                    }
            //
            //                }
            //            }
            //            if (skill == skill.characterBody.skillLocator.utility)
            //            {
            //                SS2Log.Debug("found special");
            //                if (forceToken)
            //                {
            //                    SS2Log.Debug("found force token");
            //                    if (forceToken.specialInc < stack)
            //                    {
            //                        float jankCD = body.skillLocator.special.baseRechargeInterval * cooldownPenalty;
            //                        self.skillLocator.special.finalRechargeInterval += jankCD;
            //                        forceToken.specialInc = stack;
            //                        SS2Log.Debug("finished special: " + forceToken.specialInc + " | jank: " + jankCD);
            //                    }
            //                }
            //            }
            //
            //            //if (body.skillLocator.primary)
            //            //{
            //            //    float jankCD = body.skillLocator.primary.baseRechargeInterval * cooldownPenalty;
            //            //    self.skillLocator.primary.finalRechargeInterval += jankCD;
            //            //}
            //            //if (body.skillLocator.secondary)
            //            //{
            //            //    float jankCD = body.skillLocator.secondary.baseRechargeInterval * cooldownPenalty;
            //            //    self.skillLocator.secondary.finalRechargeInterval += jankCD;
            //            //}
            //            //if (body.skillLocator.utility)
            //            //{
            //            //    float jankCD = body.skillLocator.utility.baseRechargeInterval * cooldownPenalty;
            //            //    self.skillLocator.utility.finalRechargeInterval += jankCD;
            //            //
            //            //}
            //            //if (body.skillLocator.special)
            //            //{
            //            //    float jankCD = body.skillLocator.special.baseRechargeInterval * cooldownPenalty;
            //            //    self.skillLocator.special.finalRechargeInterval += jankCD;
            //            //}
            //        }
            //    }
            //}


            //public void RecalculateStatsEnd()
            //{
            //    if (body.skillLocator)
            //    {
            //        float cooldownPenalty = MSUtil.InverseHyperbolicScaling(forcePenalty, forcePenalty, 0.9f, stack);
            //        SS2Log.Debug("Relic Stats Penalty: " + cooldownPenalty);
            //        if (body.skillLocator.primary)
            //        {
            //            float jankCD = body.skillLocator.primary.baseRechargeInterval * cooldownPenalty;
            //            SS2Log.Debug("Relic Stats PRIMARY pre - " + body.skillLocator.primary.flatCooldownReduction + " | jank: " + jankCD);
            //            body.skillLocator.primary.flatCooldownReduction -= jankCD;
            //            //body.skillLocator.primary.cooldownScale *= cooldownPenalty;
            //            SS2Log.Debug("Relic Stats PRIMARY post- " + body.skillLocator.primary.flatCooldownReduction);
            //        }
            //        if (body.skillLocator.secondary)
            //        {
            //            float jankCD = body.skillLocator.secondary.baseRechargeInterval * cooldownPenalty;
            //
            //            SS2Log.Debug("Relic Stats SECONDARY pre - " + body.skillLocator.secondary.baseRechargeStopwatch + " | jank: " + jankCD);
            //            body.skillLocator.secondary.flatCooldownReduction -= jankCD;
            //            
            //            body.skillLocator.secondary.baseRechargeStopwatch += jankCD;
            //            SS2Log.Debug("Relic Stats SECONDARY post- " + body.skillLocator.secondary.baseRechargeStopwatch);
            //        }
            //        if (body.skillLocator.utility)
            //        {
            //            float jankCD = body.skillLocator.utility.baseRechargeInterval * cooldownPenalty;
            //            //body.skillLocator.utility.flatCooldownReduction -= jankCD;
            //            SS2Log.Debug("Relic Stats UTILITY  pre - " + body.skillLocator.utility.finalRechargeInterval);
            //            //body.skillLocator.utility.cooldownScale += cooldownPenalty;
            //            body.skillLocator.utility.finalRechargeInterval += jankCD;
            //            SS2Log.Debug("Relic Stats UTILITY post - " + body.skillLocator.utility.finalRechargeInterval);
            //
            //        }
            //        if (body.skillLocator.special)
            //        {
            //            //float jankCD = body.skillLocator.special.baseRechargeInterval * cooldownPenalty;
            //            //body.skillLocator.special.flatCooldownReduction -= jankCD;
            //            SS2Log.Debug("Relic Stats SPECIAL pre - " + body.skillLocator.special.cooldownScale);
            //            body.skillLocator.special.cooldownScale *= cooldownPenalty;
            //            SS2Log.Debug("Relic Stats SPECIAL post - " + body.skillLocator.special.cooldownScale);
            //        }
            //            
            //    }
            //}
            //
            //public void RecalculateStatsStart()
            //{
            //    
            //}
            //private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody self, RecalculateStatsAPI.StatHookEventArgs args)
            //{
            //}
        }
        public class ForceToken : MonoBehaviour
        {
            //public float basePrimaryCD = 0;
            //public float baseSecondaryCD = 0;
            //public float baseUtilityCD = 0;
            //public float baseSpecialCD = 0;

            public float primaryInc = 0;
            public float secondaryInc = 0;
            public float utilityInc = 0;
            public float specialInc = 0;

            public float hilariousValue = 3691;
        }

    }

}
