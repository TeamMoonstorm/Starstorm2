using RoR2;
using RoR2.Items;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class Needles : ItemBase
    {
        private const string token = "SS2_ITEM_NEEDLES_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Needles", SS2Bundle.Items);

        //the graveyard

        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Chance for Needles to Proc. (100 = 100%)")]
        //[ConfigurableField(ConfigDesc = "Chance for Needles to Proc. (100 = 100%)")]
        //[TokenModifier(token, StatTypes.Default, 0)]
        //public static float procChance = 4f;
        //
        //[ConfigurableField(ConfigDesc = "Duration of the pricked debuff, in seconds.")]
        //[TokenModifier(token, StatTypes.Default, 1)]
        //public static float buildupDuration = 5f;
        //
        //[ConfigurableField(ConfigDesc = "Additional duration of the pricked debuff per stack, in seconds.")]
        //[TokenModifier(token, StatTypes.Default, 2)]
        //public static float buildupStack = 1f;
        //
        //[ConfigurableField(ConfigDesc = "Amount of buildup debuffs needed before the actual needles debuff gets applied")]
        //[TokenModifier(token, StatTypes.Default, 3)]
        //public static float neededBuildupAmount = 1f;
        //
        //[ConfigurableField(ConfigDesc = "Duration of the actual needles debuff, in seconds.")]
        //[TokenModifier(token, StatTypes.Default, 4)]
        //public static float needleBuffDuration = 2f;
        //
        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of the pricked debuff, in seconds.")]
        //[TokenModifier(token, StatTypes.Default, 1)]
        //public static float buildupDuration = 5f;
        //
        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Additional duration of the pricked debuff per stack, in seconds.")]
        //[TokenModifier(token, StatTypes.Default, 2)]
        //public static float buildupStack = 1f;
        //
        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of buildup debuffs needed before the actual needles debuff gets applied")]
        //[TokenModifier(token, StatTypes.Default, 3)]
        //public static float neededBuildupAmount = 1f;
        //
        //[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Duration of the actual needles debuff, in seconds.")]
        //[TokenModifier(token, StatTypes.Default, 4)]
        //public static float needleBuffDuration = 2f;

        [ConfigurableField(SS2Config.IDItem, ConfigDesc = "Amount of bonus critical chance per applied per stack. (1 = 1%")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float bonusCrit = 1;

        [ConfigurableField(ConfigDesc = "Amount of critical hits allowed per stack. (1 = 1 critical hit per stack before the buff is cleared)")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static int critsPerStack = 1;

        public sealed class Behavior : BaseItemBodyBehavior, IOnIncomingDamageOtherServerReciever //, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Needles;

            //public void OnDamageDealtServer(DamageReport report)
            //{
            //    SS2Log.Debug(report.damageInfo.crit);
            //}

            public void OnIncomingDamageOther(HealthComponent self, DamageInfo damageInfo)
            {
                //CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (damageInfo.attacker)
                {
                    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (attackerBody && !damageInfo.rejected && NetworkServer.active) // && damageInfo.procCoefficient > 0f) //then dots can't apply stacks
                    {
                        //SS2Log.Info("original if");
                        //doNeedleProc(self); 
                        //}
                        //else //what the fuck is this else for again?
                        //{
                        //SS2Log.Info("original else");
                        //P(A+B) = P(A) + P(B) - P(AB)
                        int buffCount = self.body.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup);
                        //float intendedChance = attackerBody.crit + (self.body.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup) * attackerBody.inventory.GetItemCount(SS2Content.Items.Needles)); //assuming each buff is 1% per items
                        float intendedChance = attackerBody.crit + (buffCount * bonusCrit);
                        float secondChance = (attackerBody.crit - intendedChance) / (attackerBody.crit - 100) * 100;
                        bool secondCrit = Util.CheckRoll(secondChance);
                        //if (secondCrit)
                        //{
                        //    damageInfo.crit = secondCrit;
                        //}
                        if (secondCrit && damageInfo.procCoefficient > 0f) //&& damageInfo.damageType != DamageType.DoT
                        {
                            //SS2Log.Info("second if");
                            doNeedleProc(self);
                            damageInfo.crit = secondCrit;
                        }
                        else
                        {
                            //SS2Log.Info("second else");
                            //P(A+B) = P(A) + P(B) - P(AB)
                            //intendedChance = attackerBody.crit + (self.body.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup) * attackerBody.inventory.GetItemCount(SS2Content.Items.Needles)); //assuming each buff is 1% per items
                            intendedChance = attackerBody.crit + (buffCount * bonusCrit);

                            secondChance = (attackerBody.crit - intendedChance) / (attackerBody.crit - 100) * 100;
                            secondCrit = Util.CheckRoll(secondChance);
                            //damageInfo.crit = secondCrit;
                            if (secondCrit && damageInfo.procCoefficient > 0f) //&& damageInfo.damageType != DamageType.DoT
                            {
                                //SS2Log.Info("third if");
                                damageInfo.crit = secondCrit;
                                doNeedleProc(self);
                            }
                            else
                            {
                                //SS2Log.Info("third else");
                                var tracker = self.body.gameObject.GetComponent<NeedleTracker>();
                                if (!tracker)
                                {
                                    tracker = self.body.gameObject.AddComponent<NeedleTracker>();
                                    tracker.procs = attackerBody.GetItemCount(SS2Content.Items.Needles) * critsPerStack;
                                    tracker.max = tracker.procs;
                                }
                                //self.body.AddBuff(SS2Content.Buffs.BuffNeedleBuildup);

                                SS2Log.Info("buffCount | bonusCrit " + buffCount + " | " + bonusCrit + " | " + attackerBody.crit);

                                if((buffCount * bonusCrit) + attackerBody.crit >= 100)
                                {
                                    //damageInfo.damage = damageInfo.damage * 1.1f; //eh this is weird
                                    doNeedleProc(self);
                                }

                                self.body.AddBuff(SS2Content.Buffs.BuffNeedleBuildup);
                            }
                        }
                    }
                }
                

            }

            //private void OnEnable()
            //{
            //    On.RoR2.HealthComponent.TakeDamage += NeedlesCritMod;
            //    
            //}
            //private void OnDisable()
            //{
            //    On.RoR2.HealthComponent.TakeDamage -= NeedlesCritMod;
            //}
            //
            //private void NeedlesCritMod(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
            //{
            //    CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
            //
            //    if(attackerBody && !damageInfo.rejected && NetworkServer.active)
            //    {
            //        if (damageInfo.crit) //this means needles didn't help allow the crit - it would've happened anyway
            //        {
            //            //doNeedleProc(self); 
            //        }
            //        else
            //        {
            //            //P(A+B) = P(A) + P(B) - P(AB)
            //            float intendedChance = attackerBody.crit + (self.body.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup) * attackerBody.inventory.GetItemCount(SS2Content.Items.Needles)); //assuming each buff is 1% per items
            //            float secondChance = (attackerBody.crit - intendedChance) / (attackerBody.crit - 100) * 100;
            //            bool secondCrit = Util.CheckRoll(secondChance);
            //            damageInfo.crit = secondCrit;
            //            if (damageInfo.crit)
            //            {
            //                doNeedleProc(self);
            //            }
            //            else
            //            {
            //                var tracker = self.body.gameObject.GetComponent<NeedleTracker>();
            //                if (!tracker)
            //                {
            //                    tracker = self.body.gameObject.AddComponent<NeedleTracker>();
            //                    tracker.procs = attackerBody.GetItemCount(SS2Content.Items.Needles);
            //                    tracker.max = tracker.procs;
            //                }
            //                self.body.AddBuff(SS2Content.Buffs.BuffNeedleBuildup);
            //            }
            //        }
            //    }
            //    
            //    orig(self, damageInfo);
            //}

            public void doNeedleProc(HealthComponent self)
            {
                var tracker = self.body.gameObject.GetComponent<NeedleTracker>();
                if (tracker)
                {
                    tracker.procs--;
                }
                if (!tracker || tracker.procs == 0)
                {
                    if (tracker)
                    {
                        Destroy(tracker);
                    }
                    int buffCount = self.body.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup);
                    for (int i = 0; i < buffCount; i++)
                    {
                        self.body.RemoveBuff(SS2Content.Buffs.BuffNeedleBuildup);
                    }
                }

                EffectData effectData = new EffectData
                {
                    origin = self.body.corePosition
                };
                EffectManager.SpawnEffect(HealthComponent.AssetReferences.executeEffectPrefab, effectData, transmit: true);
            }
        }

        public class NeedleTracker : MonoBehaviour
        {
            //helps keep track of the target and player responsible
            //public CharacterBody PlayerOwner;
            public int procs;
            public int max;

        }
    }
}