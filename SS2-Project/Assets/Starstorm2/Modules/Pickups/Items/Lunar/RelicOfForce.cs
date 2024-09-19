﻿using R2API;
using RoR2;
using RoR2.Items;
using System.Collections;
using UnityEngine;

using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using MSU.Config;

namespace SS2.Items
{
    public sealed class RelicOfForce : SS2Item
    {
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acRelicOfForce", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Attack speed reduction and cooldown increase per stack. (1 = 100% slower attack speed and longer cooldowns)")]
        [FormatToken("SS2_ITEM_RELICOFFORCE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
        public static float forcePenalty = .4f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Delay between additional hits. (1 = 1 second)")]
        public static float hitDelay = .2f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Increased damage per additional hits. (1 = 100%)")]
        [FormatToken("SS2_ITEM_RELICOFFORCE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 1)]
        public static float hitIncrease = .05f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Increased damage cap for additional hits. (1 = 100%)")]
        [FormatToken("SS2_ITEM_RELICOFFORCE_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float hitMax = 1f;

        public static DamageAPI.ModdedDamageType relicForceDamageType;

        public override void Initialize()
        {
            if (!SS2Main.GOTCEInstalled)
            {
                On.RoR2.GenericSkill.CalculateFinalRechargeInterval += ForceSkillFinalRecharge; //since this hook is exactly one from gotce, let's not run it twice
            }
            else
            {
                SS2Log.Info("GOTCE Compat - Not adding Force hook");
            }

            relicForceDamageType = DamageAPI.ReserveDamageType();
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }


        private float ForceSkillFinalRecharge(On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig, GenericSkill self)
        {
            float num = orig(self);
            return self.baseRechargeInterval > 0 ? Mathf.Max(num, self.baseRechargeInterval * self.cooldownScale - self.flatCooldownReduction) : 0; //lovely ternary, thank you HIFU
        }

        public sealed class Behavior : BaseItemBodyBehavior, IBodyStatArgModifier, IOnDamageDealtServerReceiver
        {
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.RelicOfForce;

            //ForceHitToken EnemyToken;

            public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
            {
                //args.damageMultAdd += damageMultiplier;
                float penalty = MSUtil.InverseHyperbolicScaling(forcePenalty, forcePenalty, 0.9f, stack);
                args.attackSpeedMultAdd -= penalty;

                //args.primaryCooldownMultAdd += penalty; //this got removed as it makes some primaries feel like absolute ass
                args.secondaryCooldownMultAdd += penalty;
                args.utilityCooldownMultAdd += penalty;
                args.specialCooldownMultAdd += penalty;
            }

            public void OnDamageDealtServer(DamageReport damageReport)
            {
                if (!damageReport.damageInfo.HasModdedDamageType(relicForceDamageType) && !damageReport.damageInfo.HasModdedDamageType(Malice.MaliceDamageType) && damageReport.damageInfo.procCoefficient > 0 && damageReport.attacker && damageReport.victimBody)
                {
                    int count = damageReport.attackerBody.inventory.GetItemCount(SS2Content.Items.RelicOfForce); //im pretty sure using stack here made the mod break and im just not having it rn, this works
                    if (count > 0)
                    {
                        var token = damageReport.victim.body.gameObject.GetComponent<ForceHitToken>();
                        if (token)
                        {
                            token.CallMoreHits(damageReport, count);
                        }
                        else
                        {
                            token = damageReport.victim.body.gameObject.AddComponent<ForceHitToken>();
                            token.CallMoreHits(damageReport, count);
                        }

                    }
                }
            }
        }

        public class ForceHitToken : MonoBehaviour
        {
            public int hitCount = 0;

            private void Start()
            {
            }

            public void CallMoreHits(DamageReport damageReport, int count)
            {
                if (hitCount * hitIncrease < hitMax)
                {
                    hitCount++;
                }
                StartCoroutine(RelicForceDelayedHits(damageReport, count));
            }

            IEnumerator RelicForceDelayedHits(DamageReport damageReport, int count)
            {
                var attacker = damageReport.attacker;
                var victim = damageReport.victimBody;
                var victimHealthComp = damageReport.victimBody.healthComponent;
                var initalHit = damageReport.damageInfo;

                float hitMult = hitCount * hitIncrease;

                for (int i = 0; i < count; i++)
                {
                    DamageInfo damageInfo = new DamageInfo();
                    damageInfo.damage = damageReport.damageDealt * hitMult;
                    damageInfo.attacker = attacker;
                    damageInfo.inflictor = initalHit.inflictor;
                    damageInfo.force = Vector3.zero;
                    damageInfo.crit = initalHit.crit;
                    damageInfo.procChainMask = initalHit.procChainMask;
                    damageInfo.procCoefficient = initalHit.procCoefficient / 2f;
                    damageInfo.position = victim.transform.position;
                    damageInfo.damageColorIndex = DamageColorIndex.Item;
                    damageInfo.damageType = initalHit.damageType;
                    damageInfo.AddModdedDamageType(RelicOfForce.relicForceDamageType);

                    yield return new WaitForSeconds(hitDelay);
                    damageInfo.position = victim.transform.position;
                    if (victim.healthComponent.alive)
                    {
                        damageInfo.position = victim.transform.position;
                        victimHealthComp.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, victimHealthComp.gameObject); //what
                        GlobalEventManager.instance.OnHitAll(damageInfo, victimHealthComp.gameObject);
                        EffectData effectData = new EffectData
                        {
                            origin = victim.transform.position
                        };
                        effectData.SetNetworkedObjectReference(victim.gameObject);
                        EffectManager.SpawnEffect(SS2Assets.LoadAsset<GameObject>("RelicOfForceHitEffect", SS2Bundle
                            .Items), effectData, transmit: true);
                    }

                }
            }

        }
    }
}

