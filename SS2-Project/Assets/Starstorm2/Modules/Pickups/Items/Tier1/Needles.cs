using RoR2;
using RoR2.Items;
using UnityEngine;
using MSU;
using System.Collections.Generic;
using RoR2.ContentManagement;
using System.Collections;
using MSU.Config;
using System;
using R2API;
using RoR2.Orbs;
using RoR2.Projectile;

namespace SS2.Items
{
    // needs sound
    public sealed class Needles : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_NEEDLES_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acNeedles", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for the debuff to be applied on hit. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float procChance = 8;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of guaranteed critical hits. (1 = 1 critical hit per stack before the buff is cleared)")]
        [FormatToken(token, 1)]
        public static int crits = 3;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of guaranteed critical hits per stack of this item. (1 = 1 critical hit per stack before the buff is cleared)")]
        [FormatToken(token, 2)]
        public static int critsPerStack = 1;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage coefficient of the mini-crit. (1 = 1%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float needleDamageCoefficient = 0.5f;
        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Damage coefficient of the mini-crit, per stack of this item. (1 = 1%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 4)]
        public static float needleDamageCoefficientPerStack = 0.25f;

        public static float needleProcCoefficient = 0.7f;
        public static float needleStrikeInterval = 0.115f;

        private static GameObject _procEffect;
        private static GameObject _critEffect;
        private static GameObject _strikeEffect;
        private static R2API.ModdedProcType needleProcType;

        private static List<CharacterBody> bodiesToRemoveNeedlesFromDude;
        public override void Initialize()
        {
            _procEffect = AssetCollection.FindAsset<GameObject>("NeedlesProcEffect");
            _critEffect = AssetCollection.FindAsset<GameObject>("NeedlesCritEffect");
            _strikeEffect = AssetCollection.FindAsset<GameObject>("NeedlesStrikeEffect");

            needleProcType = R2API.ProcTypeAPI.ReserveProcType();

            bodiesToRemoveNeedlesFromDude = new List<CharacterBody>();
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
            RoR2Application.onFixedUpdate += ProcessNeedleBuffRemovals;
        }

        // only remove one stack of needles per frame, so that shotgun-style attacks will all crit
        public static void RemoveNeedleBuff(CharacterBody body)
        {
            if (body && !bodiesToRemoveNeedlesFromDude.Contains(body))
            {
                bodiesToRemoveNeedlesFromDude.Add(body);
            }
        }
        private void ProcessNeedleBuffRemovals()
        {
            for (int i = bodiesToRemoveNeedlesFromDude.Count - 1; i >= 0; i--)
            {
                var body = bodiesToRemoveNeedlesFromDude[i];
                if (body)
                {
                    body.RemoveBuff(SS2Content.Buffs.BuffNeedleBuildup);
                }

                bodiesToRemoveNeedlesFromDude.RemoveAt(i);
            }
        }
        
        
        private void OnServerDamageDealt(DamageReport damageReport)
        {
            var damageInfo = damageReport.damageInfo;
            var attackerBody = damageReport.attackerBody;
            var victimBody = damageReport.victimBody;

            if (!attackerBody || !attackerBody.inventory)
            {
                return;
            }
            if (damageInfo.procChainMask.HasModdedProc(needleProcType))
            {
                return;
            }

            int itemCount = attackerBody.inventory.GetItemCountEffective(SS2Content.Items.Needles);
            bool hasBuff = victimBody.HasBuff(SS2Content.Buffs.BuffNeedleBuildup);

            if (hasBuff)
            {
                RemoveNeedleBuff(victimBody);

                float damageCoefficient = needleDamageCoefficient + needleDamageCoefficientPerStack * (itemCount - 1);
                float damageValue = damageInfo.damage * damageCoefficient;

                int needleStrikeCount = 1;
                if (damageInfo.crit)
                {
                    // if damageInfo.crit == true, then damageValue would have been doubled.
                    // We want to spawn 2 needles on crit, instead of 1 with double damage.
                    needleStrikeCount = 2;

                    //damageValue *= 0.5f; // nevermind no it doenst
                }

                NeedleOrb needleOrb = new NeedleOrb();
                needleOrb.origin = damageInfo.position;
                // `isCrit = true` will double the damage. we want needle strikes to do the damage stated in the description, while still counting as a crit.
                // will still be boosted by crit damage, which we want
                needleOrb.damageValue = damageValue * 0.5f; 
                needleOrb.isCrit = true;

                needleOrb.totalStrikes = needleStrikeCount;
                needleOrb.teamIndex = damageReport.attackerTeamIndex;
                needleOrb.attacker = damageInfo.attacker;
                needleOrb.procChainMask = damageInfo.procChainMask;
                needleOrb.procChainMask.AddModdedProc(needleProcType);
                needleOrb.damageType = DamageType.Generic;
                needleOrb.procCoefficient = needleProcCoefficient;
                needleOrb.damageColorIndex = DamageColorIndex.Item;
                needleOrb.strikeInterval = needleStrikeInterval;

                Vector3 hitNormal = damageInfo.position - attackerBody.corePosition;
                if (damageInfo.inflictor)
                {
                    CharacterBody CharacterBody = damageInfo.inflictor.GetComponent<CharacterBody>();
                    hitNormal = (CharacterBody ? CharacterBody.corePosition : damageInfo.inflictor.transform.position) - damageInfo.position;
                }
                needleOrb.hitNormal = hitNormal;

                if (damageInfo.inflictedHurtbox)
                {
                    needleOrb.target = damageInfo.inflictedHurtbox;
                }
                else if (victimBody.mainHurtBox)
                {
                    needleOrb.target = victimBody.mainHurtBox;
                }
                if (needleOrb.target)
                {
                    OrbManager.instance.AddOrb(needleOrb);
                }

                if (_critEffect)
                {
                    EffectManager.SimpleEffect(_critEffect, damageInfo.position, Quaternion.identity, true);
                }
            }

            if (itemCount > 0)
            {
                if (!hasBuff && Util.CheckRoll(procChance * damageInfo.procCoefficient, attackerBody.master))
                {
                    int buffCount = crits + critsPerStack * (itemCount - 1);
                    for (int i = 0; i < buffCount; i++)
                    {
                        victimBody.AddBuff(SS2Content.Buffs.BuffNeedleBuildup);
                    }

                    EffectManager.SimpleEffect(_procEffect, damageInfo.position, Quaternion.identity, true);
                }
            }
        }


        public class NeedleOrb : Orb, IOrbFixedUpdateBehavior
        {
            public float damageValue;
            public GameObject attacker;
            public GameObject inflictor;
            public int totalStrikes;
            public float strikeInterval = 0.5f;
            public TeamIndex teamIndex;
            public bool isCrit;
            public ProcChainMask procChainMask;
            public float procCoefficient = 1f;
            public DamageColorIndex damageColorIndex;
            public DamageTypeCombo damageType = DamageType.Generic;
            public Vector3 hitNormal;
            private GameObject effectPrefab;

            private float strikeStopwatch;

            private static bool strikeOnBegin = false;
            public override void Begin()
            {
                strikeStopwatch = 0f;
                effectPrefab = _strikeEffect;
                

                if (strikeOnBegin)
                {
                    duration = (totalStrikes - 1) * strikeInterval;
                    Strike();
                }
                else
                {
                    duration = totalStrikes * strikeInterval;
                }
            }

            public override void OnArrival()
            {
            }

            public void FixedUpdate()
            {
                strikeStopwatch += Time.fixedDeltaTime;
                while (strikeStopwatch > strikeInterval)
                {
                    strikeStopwatch -= strikeInterval;
                    if (target)
                    {
                        origin = target.transform.position;
                    }
                    Strike();
                }
            }

            private void Strike()
            {
                if (effectPrefab)
                {
                    Quaternion rotation = Util.QuaternionSafeLookRotation(hitNormal);
                    EffectData effectData = new EffectData
                    {
                        origin = origin,
                        rotation = rotation,
                        genericFloat = 0.1f
                    };
                    effectData.SetHurtBoxReference(target);
                    EffectManager.SpawnEffect(effectPrefab, effectData, true);
                }

                if (target)
                {
                    HealthComponent healthComponent = target.healthComponent;
                    if (healthComponent)
                    {
                        DamageInfo damageInfo = new DamageInfo();
                        damageInfo.damage = damageValue;
                        damageInfo.attacker = attacker;
                        damageInfo.inflictor = inflictor;
                        damageInfo.force = Vector3.zero;
                        damageInfo.crit = isCrit;
                        damageInfo.procChainMask = procChainMask;
                        damageInfo.procCoefficient = procCoefficient;
                        damageInfo.position = target.transform.position;
                        damageInfo.damageColorIndex = damageColorIndex;
                        damageInfo.damageType = damageType;
                        damageInfo.inflictedHurtbox = target;
                        healthComponent.TakeDamage(damageInfo);
                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                    }
                }
            }
        }
    }
}