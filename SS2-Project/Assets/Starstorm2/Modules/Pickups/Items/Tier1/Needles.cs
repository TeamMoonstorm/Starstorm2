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

namespace SS2.Items
{
    // needs sound
    public sealed class Needles : SS2Item, IContentPackModifier
    {
        private const string token = "SS2_ITEM_NEEDLES_DESC";
        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<ItemAssetCollection>("acNeedles", SS2Bundle.Items);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for the debuff to be applied on hit. (1 = 1%)")]
        [FormatToken(token, 0)]
        public static float critChancePerStack = 5;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Chance for the debuff to be applied on hit. (1 = 1%)")]
        [FormatToken(token, 1)]
        public static float needleStrikeChance = 10f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of guaranteed critical hits per stack of this item. (1 = 1 critical hit per stack before the buff is cleared)")]
        [FormatToken(token, 2)]
        public static int needleStrikeCount = 3;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of guaranteed critical hits per stack of this item. (1 = 1 critical hit per stack before the buff is cleared)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 3)]
        public static float needleDamageCoefficient = 0.2f;

        public static float needleProcCoefficient = 0.3f;
        public static float needleStrikeInterval = 0.1f;

        private static GameObject _procEffect;
        private static GameObject _critEffect;
        private static GameObject _strikeEffect;

        private static R2API.ModdedProcType needleProcType;

        private BuffDef _buffNeedleBuildup; //{ get; } = SS2Assets.LoadAsset<BuffDef>("BuffNeedleBuildup", SS2Bundle.Items);

        public override void Initialize()
        {
            _procEffect = AssetCollection.FindAsset<GameObject>("NeedlesProcEffect");
            _critEffect = AssetCollection.FindAsset<GameObject>("NeedlesCritEffect");
            _strikeEffect = AssetCollection.FindAsset<GameObject>("NeedlesStrikeEffect");

            needleProcType = R2API.ProcTypeAPI.ReserveProcType();

            R2API.RecalculateStatsAPI.GetStatCoefficients += GetStatCoefficients;
            GlobalEventManager.onServerDamageDealt += OnServerDamageDealt;
        }

        private void GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.inventory)
            {
                int itemCount = sender.inventory.GetItemCountEffective(SS2Content.Items.Needles);
                args.critAdd += itemCount * critChancePerStack;
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

            if (damageInfo.crit && !damageInfo.procChainMask.HasModdedProc(needleProcType))
            {
                int itemCount = attackerBody.inventory.GetItemCountEffective(SS2Content.Items.Needles);
                if (itemCount > 0)
                {
                    float procChance = needleStrikeChance * damageInfo.procCoefficient;
                    if (Util.CheckRoll(procChance, damageReport.attackerMaster))
                    {
                        float damageValue = damageInfo.damage * (needleDamageCoefficient * itemCount); 

                        NeedleOrb needleOrb = new NeedleOrb();
                        needleOrb.origin = damageInfo.position;
                        needleOrb.damageValue = damageValue;
                        needleOrb.isCrit = damageInfo.crit;
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

                        if (_procEffect)
                        {
                            EffectManager.SimpleEffect(_procEffect, damageInfo.position, Quaternion.identity, true);
                        }
                    }
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

            public override void Begin()
            {
                strikeStopwatch = 0f;
                duration = (totalStrikes - 1) * strikeInterval;
                effectPrefab = _strikeEffect;
                Strike();
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
                        Strike();
                    }
                }
            }

            private void Strike()
            {
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
   
                        if (effectPrefab)
                        {
                            Quaternion rotation = Util.QuaternionSafeLookRotation(hitNormal);
                            EffectData effectData = new EffectData
                            {
                                origin = target.transform.position,
                                rotation = rotation,
                                genericFloat = 0.1f
                            };
                            effectData.SetHurtBoxReference(target);
                            EffectManager.SpawnEffect(effectPrefab, effectData, true);
                        }
                    }
                }
            }
        }
    }
}