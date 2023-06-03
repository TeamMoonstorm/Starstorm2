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

            /*[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Bonus radius of malice per stack, in meters")]
 * [TokenModifier(token, StatTypes.Default, 3)]
public static float radiusStack = 1f;*/

            /*[ConfigurableField(SS2Config.IDItem, ConfigDesc = "Total damage each malice bounce after the first carries over (1 = 100%)")]
 * [TokenModifier(token, StatTypes.Percentage, 4)]
public static float scaleCoeff = 0.55f;*/

            // oldish malace code
            /*if (report.damageInfo.procCoefficient > 0)      //theoretically the bounces should never happen since they're proc 0, but this works somehow
                {

                    //To-Do: Custom Orb
                    LightningOrb malOrb = new LightningOrb();
            malOrb.bouncesRemaining = (int) Math.Truncate((stack - 1) / Mathf.Max(bounceStack, 1));

                    malOrb.range = report.victimBody.radius + radiusBase + (radiusStack * (stack - 1));    //unsure if base enemy radius is recalculated for bounces, probably not that important

                    malOrb.damageCoefficientPerBounce = Malice.scaleCoeffdamageCoeff;
                    malOrb.damageValue = report.damageInfo.damage* Malice.damageCoeff;

            malOrb.lightningType = LightningOrb.LightningType.RazorWire;            //this controls the VFX but setting it to nothing defaults to ukulele.
                                                                                            //can we add to the list of VFX in LightningOrb.LightningType?
                    malOrb.canBounceOnSameTarget = false;
                    malOrb.damageType = DamageType.Generic;
                    malOrb.isCrit = false;
                    malOrb.damageColorIndex = DamageColorIndex.Item;
                    malOrb.procCoefficient = 0f;
                    malOrb.origin = report.victimBody.corePosition;
                    malOrb.teamIndex = report.attackerTeamIndex;
                    malOrb.bouncedObjects = new List<HealthComponent>
                    {
                        report.victim
        };
        malOrb.teamIndex = report.attacker.GetComponent<TeamComponent>().teamIndex;
                    HurtBox hurtbox = malOrb.PickNextTarget(report.damageInfo.position);
                    if (hurtbox)
                    {
                        malOrb.target = hurtbox;
                        OrbManager.instance.AddOrb(malOrb);
                    }*/


            // OLD MALICE CODE BELOW - g

            //    public void OnDamageDealtServer(DamageReport report)
            //    {
            //        //TODO: custom proc type
            //        if (report.damageInfo.procChainMask.HasProc(maliceProcType) || report.damageInfo.damageType.HasFlag(DamageType.DoT) || report.damageInfo.damageType.HasFlag(DamageType.AOE))
            //            return;
            //
            //      EffectData maliceData = new EffectData();
            //      if (report.victim)
            //          maliceData.origin = report.victim.transform.position;
            //      else
            //          maliceData.origin = body.corePosition;
            //      maliceData.scale = radius;

            //Todo:
            //EffectManager.SpawnEffect(maliceEffect, maliceData, true);

            //      List<DamageInfo> damages = new List<DamageInfo>();
            //      DamageInfo previousDamageInfo = report.damageInfo;
            //      for (int i = 0; i < stack; i++)
            //      {
            //          damages.Add(GetDamageInfo(previousDamageInfo));
            //          previousDamageInfo = damages[i];
            //      }

            //      BlastAttack area = new BlastAttack();
            //      area.radius = radius;
            //      area.attacker = body.gameObject;
            //      area.inflictor = body.gameObject;
            //      area.teamIndex = body.teamComponent.teamIndex;
            //      area.attackerFiltering = AttackerFiltering.NeverHit;
            //      area.position = report.damageInfo.position;

            //      int hits = 0;
            //      var hitPoints = area.CollectHits()
            //                          .Distinct()
            //                          .GroupBy(hit => hit.hurtBox.healthComponent)
            //                          .Where(group => group.Key != report.victimBody.healthComponent)
            //                          .Select(group => group.OrderBy(hit => hit.distanceSqr).First())
            //                          .OrderBy(hit => hit.distanceSqr)
            //                          .ToArray();



            //      for (int i = 0; hits <= stack && i < hitPoints.Length; i++)
            //      {
            //          damages[hits].position = hitPoints[i].hitPosition;
            //          damages[hits].procChainMask.AddProc(maliceProcType);
            //          hitPoints[i].hurtBox.healthComponent.TakeDamage(damages[hits]);
            //          hits++;
            //      }
            //  }

            //  public DamageInfo GetDamageInfo(DamageInfo previousDamageInfo)
            //  {
            //      DamageInfo damageInfo = new DamageInfo();
            //      damageInfo.attacker = body.gameObject;
            //      damageInfo.crit = false;
            //      if (previousDamageInfo.crit)
            //          damageInfo.crit = body.RollCrit();
            //      damageInfo.damage = previousDamageInfo.damage * damageChainCoefficient;
            //      damageInfo.damageColorIndex = DamageColorIndex.Nearby;
            //      damageInfo.damageType = previousDamageInfo.damageType;
            //      damageInfo.dotIndex = DotController.DotIndex.None;

            //      ProcChainMask procMask = previousDamageInfo.procChainMask;
            //      Array allProcs = typeof(ProcType).GetEnumValues();
            //      for (int i = 0; i < allProcs.Length; i++)
            //      {
            //          ProcType proc = (ProcType)allProcs.GetValue(i);
            //          if (procMask.HasProc(proc) && !Util.CheckRoll(damageChainCoefficient, body.master))
            //              procMask.RemoveProc(proc);
            //      }
            //      damageInfo.procChainMask = procMask;

            //      return damageInfo;
            //  }
        }
    }
}
