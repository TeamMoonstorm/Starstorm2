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
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Needles");

        [ConfigurableField(ConfigDesc = "Chance for Needles to Proc. (100 = 100%)")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float procChance = 4f;

        [ConfigurableField(ConfigDesc = "Duration of the pricked debuff, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static float buildupDuration = 5f;

        [ConfigurableField(ConfigDesc = "Additional duration of the pricked debuff per stack, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 2)]
        public static float buildupStack = 1f;

        [ConfigurableField(ConfigDesc = "Amount of buildup debuffs needed before the actual needles debuff gets applied")]
        [TokenModifier(token, StatTypes.Default, 3)]
        public static float neededBuildupAmount = 1f;

        [ConfigurableField(ConfigDesc = "Duration of the actual needles debuff, in seconds.")]
        [TokenModifier(token, StatTypes.Default, 4)]
        public static float needleBuffDuration = 2f;

        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Needles;

            public void OnDamageDealtServer(DamageReport report)
            {
                SS2Log.Debug(report.damageInfo.crit);
            }

            private void OnEnable()
            {
                On.RoR2.HealthComponent.TakeDamage += NeedlesCritMod;
                
            }
            private void OnDisable()
            {
                On.RoR2.HealthComponent.TakeDamage -= NeedlesCritMod;
            }

            private void NeedlesCritMod(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();

                if(attackerBody && !damageInfo.rejected && NetworkServer.active)
                {
                    if (damageInfo.crit)
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
                        //SS2Log.Debug("normal remove: ");
                    }
                    else
                    {
                        //SS2Log.Debug("crit: " + attackerBody.crit);
                        float intendedChance = attackerBody.crit + (self.body.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup) * attackerBody.inventory.GetItemCount(SS2Content.Items.Needles)); //assuming each buff is 1% per items
                        //SS2Log.Debug("intended: " + intendedChance);
                        float secondChance = (attackerBody.crit - intendedChance) / (attackerBody.crit - 100) * 100;
                        //SS2Log.Debug("second: " + secondChance);
                        bool secondCrit = Util.CheckRoll(secondChance);
                        //SS2Log.Debug("critted: " + secondCrit);
                        damageInfo.crit = secondCrit;
                        if (damageInfo.crit)
                        {
                            //SS2Log.Debug("backup remove: ");
                            var tracker = self.body.gameObject.GetComponent<NeedleTracker>();
                            if (tracker)
                            {
                                tracker.procs--;
                            }
                            if(!tracker || tracker.procs == 0)
                            {
                                if (tracker)
                                {
                                    Destroy(tracker);
                                }
                                int buffCount = self.body.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup);
                                for(int i = 0; i < buffCount; i++)
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
                        else
                        {
                            var tracker = self.body.gameObject.GetComponent<NeedleTracker>();
                            if (!tracker)
                            {
                                tracker = self.body.gameObject.AddComponent<NeedleTracker>();
                                tracker.procs = attackerBody.GetItemCount(SS2Content.Items.Needles);
                                tracker.max = tracker.procs;
                            }
                            self.body.AddBuff(SS2Content.Buffs.BuffNeedleBuildup);
                        }
                    }
                }
                
                orig(self, damageInfo);
            }
            //public void OnDamageDealtServer(DamageReport report)
            //{
            //    if (Util.CheckRoll((procChance * stack) * report.damageInfo.procCoefficient, body.master))
            //    {
            //        if (!report.victimBody.HasBuff(SS2Content.Buffs.BuffNeedle))
            //        {
            //            report.victimBody.AddTimedBuff(SS2Content.Buffs.BuffNeedle, needleBuffDuration * stack);
            //            //report.victimBody.AddTimedBuff(SS2Content.Buffs.BuffNeedleBuildup, ((buildupDuration + (stack - 1) * buildupStack)) * report.damageInfo.procCoefficient);
            //            //if (report.victimBody.GetBuffCount(SS2Content.Buffs.BuffNeedleBuildup) >= neededBuildupAmount)
            //            //{
            //            //    report.victimBody.ClearTimedBuffs(SS2Content.Buffs.BuffNeedleBuildup);
            //            //    report.victimBody.AddTimedBuff(SS2Content.Buffs.BuffNeedle, needleBuffDuration);
            //            //}
            //        }
            //    }
            //}
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