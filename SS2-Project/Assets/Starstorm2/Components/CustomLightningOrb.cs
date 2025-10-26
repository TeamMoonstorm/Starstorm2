using System.Collections.Generic;
using System.Linq;
using RoR2;
using UnityEngine;
using RoR2.Orbs;
using static SS2.Items.ErraticGadget;
using MSU;
namespace SS2
{
    // copy of ror2.orbs.lgihtningorb
    public class CustomLightningOrb : Orb
    {
        public GameObject orbEffectPrefab;
        public float duration = 0.033f;
        public bool canProcGadget;
        public bool canBounceOnSameTarget;
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
        public float range = 20f;
        public float damageCoefficientPerBounce = 1f;
        public DamageType damageType;

        private BullseyeSearch search;
        public override void Begin()
        {
            base.duration = duration;

            EffectData effectData = new EffectData
            {
                origin = this.origin,
                genericFloat = base.duration
            };
            effectData.SetHurtBoxReference(this.target);
            EffectManager.SpawnEffect(orbEffectPrefab, effectData, true);
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
                    healthComponent.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);
                }

                this.bouncedObjects.Add(this.target.healthComponent);

                if (this.bouncesRemaining > 0)
                {
                    if (this.bouncedObjects != null)
                    {                      
                        if (this.canBounceOnSameTarget)
                        {
                            this.bouncedObjects.Clear();
                        }
                    }
                    HurtBox hurtBox = this.PickNextTarget(this.target.transform.position, healthComponent);
                    if (hurtBox)
                    {
                        CustomLightningOrb lightningOrb = new CustomLightningOrb();
                        lightningOrb.duration = duration;
                        lightningOrb.canProcGadget = this.canProcGadget;
                        lightningOrb.canBounceOnSameTarget = this.canBounceOnSameTarget;
                        lightningOrb.search = this.search;
                        lightningOrb.origin = this.target.transform.position;
                        lightningOrb.target = hurtBox;
                        lightningOrb.attacker = this.attacker;
                        lightningOrb.inflictor = this.inflictor;
                        lightningOrb.teamIndex = this.teamIndex;
                        lightningOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
                        lightningOrb.bouncesRemaining = this.bouncesRemaining - 1;
                        lightningOrb.isCrit = this.isCrit;
                        lightningOrb.bouncedObjects = this.bouncedObjects;
                        lightningOrb.procChainMask = this.procChainMask;
                        lightningOrb.procCoefficient = this.procCoefficient;
                        lightningOrb.damageColorIndex = this.damageColorIndex;
                        lightningOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
                        lightningOrb.range = this.range;
                        lightningOrb.damageType = this.damageType;
                        lightningOrb.orbEffectPrefab = this.orbEffectPrefab;
                        OrbManager.instance.AddOrb(lightningOrb);
                        return;
                    }
                    
                }
                // only when no new target
                if (canProcGadget)
                {
                    HashSet<HealthComponent> uniqueObjects = new HashSet<HealthComponent>();
                    for (int i = 0; i < this.bouncedObjects.Count; i++)
                    {
                        uniqueObjects.Add(this.bouncedObjects[i]);
                    }
                    int bounces = uniqueObjects.Count;
                    HurtBox hurtBox = this.PickNextTarget(this.target.transform.position, this.target.healthComponent);
                    GadgetLightningOrb lightningOrb = new GadgetLightningOrb();
                    lightningOrb.search = this.search;
                    lightningOrb.origin = this.target.transform.position;
                    lightningOrb.target = hurtBox;
                    lightningOrb.attacker = this.attacker;
                    lightningOrb.inflictor = this.inflictor;
                    lightningOrb.teamIndex = this.teamIndex;
                    lightningOrb.damageValue = this.damageValue * this.damageCoefficientPerBounce;
                    lightningOrb.bouncesRemaining = bounces; // doubles bounces
                    lightningOrb.isCrit = this.isCrit;
                    lightningOrb.bouncedObjects = new List<HealthComponent>();// { this.target.healthComponent };
                    lightningOrb.procChainMask = this.procChainMask;
                    lightningOrb.procCoefficient = this.procCoefficient;
                    lightningOrb.damageColorIndex = this.damageColorIndex;
                    lightningOrb.damageCoefficientPerBounce = this.damageCoefficientPerBounce;
                    lightningOrb.range = this.range;
                    lightningOrb.damageType = this.damageType;
                    lightningOrb.canBounceOnSameTarget = this.canBounceOnSameTarget;
                    OrbManager.instance.AddOrb(lightningOrb);

                    EffectManager.SimpleEffect(SS2.Items.ErraticGadget._procEffectPrefab, this.target.transform.position, Quaternion.identity, true);
                }

            }
        }
        public HurtBox PickNextTarget(Vector3 position, HealthComponent currentVictim)
        {
            if (this.search == null)
            {
                this.search = new BullseyeSearch();
            }
            float range = this.range;
            if (currentVictim && currentVictim.body)
            {
                this.bouncedObjects.Add(currentVictim);
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
            if (hurtBox && hurtBox.healthComponent.alive)
            {
                this.bouncedObjects.Add(hurtBox.healthComponent);
            }
            return hurtBox;
        }
       
    }
}
