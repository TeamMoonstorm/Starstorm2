using MSU.Config;
using MSU;
using R2API;
using RoR2;
using SS2.Items;
using SS2;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2.ContentManagement;
using RoR2.Items;
using RoR2.Orbs;

namespace SS2.Items
{
    //public class VoidMalice : SS2VoidItem
    //{
    //    private const string token = "SS2_ITEM_VOID_MALICE_DESC";
    //    public override SS2AssetRequest AssetRequest()
    //    {
    //        return SS2Assets.LoadAssetAsync<ItemAssetCollection>("acVoidMalice", SS2Bundle.Items);
    //    }

    //    [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Total damage each Malice bounce deals. (1 = 100%)")]
    //    [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 0)]
    //    public static float damageCoeff = 0.25f;

    //    [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Number of bounces per stack.")]
    //    [FormatToken(token, 1)]
    //    public static int bounceStack = 1;

    //    [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius of Malice, in meters.")]
    //    [FormatToken(token, 2)]
    //    public static float radiusBase = 12f;

    //    [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Radius of Malice per stack, in meters.")]
    //    [FormatToken(token, 3)]
    //    public static float radiusPerStack = 2.4f;



    //    [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, ConfigDescOverride = "Proc coefficient of damage dealt by Malice.")]
    //    public static float procCo = 0.1f;

    //    //damage types should not be used as a substitute for proper proc chain masks, but it works here
    //    public static DamageAPI.ModdedDamageType MaliceDamageType { get; private set; }

    //    //a proc chain mask of proc types that shouldn't invalidate malice
    //    public static ProcChainMask ignoredProcs;

    //    public static GameObject maliceOrbEffectPrefab;

    //    public override void Initialize()
    //    {
    //        maliceOrbEffectPrefab = AssetCollection.FindAsset<GameObject>("MaliceOrbEffect");
    //        MaliceDamageType = DamageAPI.ReserveDamageType();
    //        ignoredProcs.AddProc(ProcType.Backstab);
    //    }

    //    public override bool IsAvailable(ContentPack contentPack)
    //    {
    //        return true;
    //    }

    //    public override List<ItemDef> GetInfectableItems()
    //    {
    //        return new List<ItemDef>
    //        {
    //            SS2Content.Items.Malice
    //        };
    //    }

    //    public sealed class Behavior : BaseItemBodyBehavior, IOnDamageDealtServerReceiver
    //    {

    //        [ItemDefAssociation]
    //        private static ItemDef GetItemDef() => SS2Content.Items.Malice;
    //        public void OnDamageDealtServer(DamageReport report)
    //        {
    //            DamageInfo damageInfo = report.damageInfo;

    //            //damage has proc co and damage info does not contained any banned proc types
    //            if (damageInfo.procCoefficient > 0 && (damageInfo.procChainMask.mask & ~ignoredProcs.mask) == 0U && !damageInfo.HasModdedDamageType(MaliceDamageType))
    //            {
    //                VoidMaliceOrb malOrb = new VoidMaliceOrb();
    //                malOrb.bouncesRemaining = bounceStack * stack - 1;
    //                malOrb.baseRange = radiusBase + radiusPerStack * (stack - 1);
    //                malOrb.damageCoefficientPerBounce = 1f;
    //                malOrb.damageValue = damageInfo.damage * Malice.damageCoeff;
    //                malOrb.damageType = DamageType.Generic;
    //                malOrb.isCrit = damageInfo.crit;
    //                malOrb.damageColorIndex = DamageColorIndex.Void;
    //                malOrb.procCoefficient = procCo * damageInfo.procCoefficient;
    //                malOrb.origin = report.damageInfo.position;
    //                malOrb.teamIndex = body.teamComponent.teamIndex;
    //                malOrb.attacker = base.gameObject;
    //                malOrb.procChainMask = damageInfo.procChainMask;
    //                malOrb.bouncedObjects = new List<HealthComponent>
    //                {
    //                    report.victim
    //                };
    //                HurtBox hurtbox = malOrb.PickNextTarget(damageInfo.position, report.victim);
    //                if (hurtbox)
    //                {
    //                    malOrb.target = hurtbox;
    //                    OrbManager.instance.AddOrb(malOrb);
    //                }
    //            }
    //        }
    //        public class VoidMaliceOrb : Orb
    //        {
    //            public float damageValue;

    //            public GameObject attacker;

    //            public GameObject inflictor;

    //            public int bouncesRemaining;

    //            public List<HealthComponent> bouncedObjects;

    //            public TeamIndex teamIndex;

    //            public bool isCrit;

    //            public ProcChainMask procChainMask;

    //            public float procCoefficient = 1f;

    //            public DamageColorIndex damageColorIndex;

    //            public float baseRange = 20f;

    //            public float damageCoefficientPerBounce = 1f;

    //            public DamageType damageType;

    //            private BullseyeSearch search;

    //            public override void Begin()
    //            {
    //                base.duration = 0.2f;

    //                EffectData effectData = new EffectData
    //                {
    //                    origin = this.origin,
    //                    genericFloat = base.duration
    //                };
    //                effectData.SetHurtBoxReference(this.target);

    //                EffectManager.SpawnEffect(maliceOrbEffectPrefab, effectData, true);
    //            }

    //            public override void OnArrival()
    //            {
    //                if (this.target)
    //                {
    //                    HealthComponent healthComponent = this.target.healthComponent;
    //                    if (healthComponent)
    //                    {
    //                        DamageInfo damageInfo = new DamageInfo();
    //                        damageInfo.damage = this.damageValue;
    //                        damageInfo.attacker = this.attacker;
    //                        damageInfo.inflictor = this.inflictor;
    //                        damageInfo.force = Vector3.zero;
    //                        damageInfo.crit = this.isCrit;
    //                        damageInfo.procChainMask = this.procChainMask;
    //                        damageInfo.procCoefficient = this.procCoefficient;
    //                        damageInfo.position = this.target.transform.position;
    //                        damageInfo.damageColorIndex = this.damageColorIndex;
    //                        damageInfo.damageType = this.damageType;
    //                        damageInfo.AddModdedDamageType(MaliceDamageType);
    //                        healthComponent.TakeDamage(damageInfo);
    //                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
    //                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
    //                    }
    //                    if (this.bouncesRemaining > 0)
    //                    {
    //                        if (this.bouncedObjects != null)
    //                        {
    //                            this.bouncedObjects.Add(this.target.healthComponent);
    //                        }
    //                        HurtBox hurtBox = this.PickNextTarget(this.target.transform.position, healthComponent);
    //                        if (hurtBox)
    //                        {
    //                            MaliceOrb maliceOrb = new MaliceOrb();
    //                            maliceOrb.search = this.search;
    //                            maliceOrb.origin = this.target.transform.position;
    //                            maliceOrb.target = hurtBox;
    //                            maliceOrb.attacker = this.attacker;
    //                            maliceOrb.inflictor = this.inflictor;
    //                            maliceOrb.teamIndex = this.teamIndex;
    //                            maliceOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
    //                            maliceOrb.bouncesRemaining = this.bouncesRemaining - 1;
    //                            maliceOrb.isCrit = this.isCrit;
    //                            maliceOrb.bouncedObjects = this.bouncedObjects;
    //                            maliceOrb.procChainMask = this.procChainMask;
    //                            maliceOrb.procCoefficient = this.procCoefficient;
    //                            maliceOrb.damageColorIndex = this.damageColorIndex;
    //                            maliceOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
    //                            maliceOrb.baseRange = this.baseRange;
    //                            maliceOrb.damageType = this.damageType;
    //                            OrbManager.instance.AddOrb(maliceOrb);
    //                        }
    //                    }

    //                }
    //            }
    //            public HurtBox PickNextTarget(Vector3 position, HealthComponent currentVictim)
    //            {
    //                if (this.search == null)
    //                {
    //                    this.search = new BullseyeSearch();
    //                }
    //                float range = baseRange;
    //                if (currentVictim && currentVictim.body)
    //                {
    //                    range += currentVictim.body.radius;
    //                }
    //                this.search.searchOrigin = position;
    //                this.search.searchDirection = Vector3.zero;
    //                this.search.teamMaskFilter = TeamMask.allButNeutral;
    //                this.search.teamMaskFilter.RemoveTeam(this.teamIndex);
    //                this.search.filterByLoS = false;
    //                this.search.sortMode = BullseyeSearch.SortMode.Distance;
    //                this.search.maxDistanceFilter = range;
    //                this.search.RefreshCandidates();
    //                HurtBox hurtBox = (from v in this.search.GetResults()
    //                                   where !this.bouncedObjects.Contains(v.healthComponent)
    //                                   select v).FirstOrDefault<HurtBox>();
    //                if (hurtBox)
    //                {
    //                    this.bouncedObjects.Add(hurtBox.healthComponent);
    //                }
    //                return hurtBox;
    //            }


    //        }
    //    }
    //}
}
