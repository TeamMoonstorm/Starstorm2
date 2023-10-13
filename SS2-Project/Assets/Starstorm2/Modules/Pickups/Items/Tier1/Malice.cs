using RoR2;
using RoR2.Items;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using R2API;
using Moonstorm.Components;

namespace Moonstorm.Starstorm2.Items
{
    public sealed class Malice : ItemBase
    {
        private const string token = "SS2_ITEM_MALICE_DESC";
        public override ItemDef ItemDef { get; } = SS2Assets.LoadAsset<ItemDef>("Malice", SS2Bundle.Items);

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Radius of Malice, in meters.")]
        [TokenModifier(token, StatTypes.Default, 0)]
        public static float radiusBase = 13f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Total damage each Malice bounce deals. (1 = 100%)")]
        [TokenModifier(token, StatTypes.MultiplyByN, 2, "100")]
        public static float damageCoeff = 0.35f;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Number of bounces per stack.")]
        [TokenModifier(token, StatTypes.Default, 1)]
        public static int bounceStack = 1;

        [RooConfigurableField(SS2Config.IDItem, ConfigDesc = "Proc coefficient of damage dealt by Malice.")]
        public static float procCo = 0.2f;

        //damage types should not be used as a substitute for proper proc chain masks, but it works here
        public static DamageAPI.ModdedDamageType maliceDamageType;

        //a proc chain mask of proc types that shouldn't invalidate malice
        public static ProcChainMask ignoredProcs;

        public static GameObject maliceOrbEffectPrefab;

        public override void Initialize()
        {
            base.Initialize();
            maliceDamageType = DamageAPI.ReserveDamageType();
            ignoredProcs.AddProc(ProcType.Backstab);

            maliceOrbEffectPrefab = SS2Assets.LoadAsset<GameObject>("MaliceOrbEffect", SS2Bundle.Items);
            /*MaterialControllerComponents.HGCloudRemapController hGCloudRemapController = maliceOrbEffectPrefab.AddComponent<MaterialControllerComponents.HGCloudRemapController>();
            LineRenderer lineRenderer = maliceOrbEffectPrefab.GetComponentInChildren<LineRenderer>();
            hGCloudRemapController.renderer = lineRenderer;
            hGCloudRemapController.material = lineRenderer.material;*/
        }
        public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
        {
            
            [ItemDefAssociation]
            private static ItemDef GetItemDef() => SS2Content.Items.Malice;
            public void OnDamageDealtServer(DamageReport report)
            {
                DamageInfo damageInfo = report.damageInfo;

                //damage has proc co, body has team comp, and damage info does not contained any banned proc types
                if (damageInfo.procCoefficient > 0 && body.teamComponent && (damageInfo.procChainMask.mask & ~ignoredProcs.mask) == 0U && !damageInfo.HasModdedDamageType(maliceDamageType))      
                {
                    MaliceOrb malOrb = new MaliceOrb();
                    malOrb.bouncesRemaining = bounceStack * stack - 1;
                    malOrb.baseRange = radiusBase;
                    malOrb.damageCoefficientPerBounce = 1f;
                    malOrb.damageValue = damageInfo.damage * Malice.damageCoeff;
                    malOrb.damageType = DamageType.Generic;
                    malOrb.isCrit = damageInfo.crit;
                    malOrb.damageColorIndex = DamageColorIndex.Void;
                    malOrb.procCoefficient = procCo;
                    malOrb.origin = report.victimBody.corePosition;
                    malOrb.teamIndex = body.teamComponent.teamIndex;
                    malOrb.attacker = base.gameObject;
                    malOrb.procChainMask = damageInfo.procChainMask;
                    malOrb.bouncedObjects = new List<HealthComponent>
                    {
                        report.victim
                    };
                    HurtBox hurtbox = malOrb.PickNextTarget(damageInfo.position, report.victim);
                    if (hurtbox)
                    {
                        malOrb.target = hurtbox;
                        OrbManager.instance.AddOrb(malOrb);
                    }
                }
            }
            //basically a LightningOrb ripoff, but supports custom behaviour and vfx
            public class MaliceOrb : Orb
            {
                public override void Begin()
                {
                    base.duration = 0.2f;

                    EffectData effectData = new EffectData
                    {
                        origin = this.origin,
                        genericFloat = base.duration
                    };
                    effectData.SetHurtBoxReference(this.target);

                    EffectManager.SpawnEffect(maliceOrbEffectPrefab, effectData, true);
                }

                public override void OnArrival()
                {
                    if (this.target)
                    {
                        HealthComponent healthComponent = this.target.healthComponent;
                        if (healthComponent)
                        {
                            DamageInfo damageInfo = new DamageInfo();
                            damageInfo.damage = this.damageValue;
                            damageInfo.attacker = this.attacker;
                            damageInfo.inflictor = this.inflictor;
                            damageInfo.force = Vector3.zero;
                            damageInfo.crit = this.isCrit;
                            damageInfo.procChainMask = this.procChainMask;
                            damageInfo.procCoefficient = this.procCoefficient;
                            damageInfo.position = this.target.transform.position;
                            damageInfo.damageColorIndex = this.damageColorIndex;
                            damageInfo.damageType = this.damageType;
                            damageInfo.AddModdedDamageType(maliceDamageType);
                            healthComponent.TakeDamage(damageInfo);
                            GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                            GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                        }
                        if (this.bouncesRemaining > 0)
                        {
                            if (this.bouncedObjects != null)
                            {
                                this.bouncedObjects.Add(this.target.healthComponent);
                            }
                            HurtBox hurtBox = this.PickNextTarget(this.target.transform.position, healthComponent);
                            if (hurtBox)
                            {
                                MaliceOrb maliceOrb = new MaliceOrb();
                                maliceOrb.search = this.search;
                                maliceOrb.origin = this.target.transform.position;
                                maliceOrb.target = hurtBox;
                                maliceOrb.attacker = this.attacker;
                                maliceOrb.inflictor = this.inflictor;
                                maliceOrb.teamIndex = this.teamIndex;
                                maliceOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
                                maliceOrb.bouncesRemaining = this.bouncesRemaining - 1;
                                maliceOrb.isCrit = this.isCrit;
                                maliceOrb.bouncedObjects = this.bouncedObjects;
                                maliceOrb.procChainMask = this.procChainMask;
                                maliceOrb.procCoefficient = this.procCoefficient;
                                maliceOrb.damageColorIndex = this.damageColorIndex;
                                maliceOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
                                maliceOrb.baseRange = this.baseRange;
                                maliceOrb.damageType = this.damageType;
                                OrbManager.instance.AddOrb(maliceOrb);
                            }
                        }

                    }
                }
                public HurtBox PickNextTarget(Vector3 position, HealthComponent currentVictim)
                {
                    if (this.search == null)
                    {
                        this.search = new BullseyeSearch();
                    }
                    float range = baseRange;
                    if(currentVictim && currentVictim.body)
                    {
                        range += currentVictim.body.radius;
                    }
                    this.search.searchOrigin = position;
                    this.search.searchDirection = Vector3.zero;
                    this.search.teamMaskFilter = TeamMask.allButNeutral;
                    this.search.teamMaskFilter.RemoveTeam(this.teamIndex);
                    this.search.filterByLoS = false;
                    this.search.sortMode = BullseyeSearch.SortMode.Distance;
                    this.search.maxDistanceFilter = range;
                    this.search.RefreshCandidates();
                    HurtBox hurtBox = (from v in this.search.GetResults()
                                       where !this.bouncedObjects.Contains(v.healthComponent)
                                       select v).FirstOrDefault<HurtBox>();
                    if (hurtBox)
                    {
                        this.bouncedObjects.Add(hurtBox.healthComponent);
                    }
                    return hurtBox;
                }

                public float damageValue;

                public GameObject attacker;

                public GameObject inflictor;

                public int bouncesRemaining;

                public List<HealthComponent> bouncedObjects;

                public TeamIndex teamIndex;

                public bool isCrit;

                public ProcChainMask procChainMask;

                public float procCoefficient = 1f;

                public DamageColorIndex damageColorIndex;

                public float baseRange = 20f;

                public float damageCoefficientPerBounce = 1f;

                public DamageType damageType;

                private BullseyeSearch search;
            }
        }
    }
}
