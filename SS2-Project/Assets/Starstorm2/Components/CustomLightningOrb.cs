using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using RoR2.Orbs;
using R2API;
namespace Moonstorm.Starstorm2.Components
{
    public class CustomLightningOrb : Orb
    {
        public GameObject orbEffectPrefab;
        public override void Begin()
        {
            base.duration = 0.033f;

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
                if (this.bouncesRemaining > 0)
                {
                    if (this.bouncedObjects != null)
                    {
                        this.bouncedObjects.Add(this.target.healthComponent);
                    }
                    HurtBox hurtBox = this.PickNextTarget(this.target.transform.position, healthComponent);
                    if (hurtBox)
                    {
                        CustomLightningOrb lightningOrb = new CustomLightningOrb();
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
                        lightningOrb.baseRange = this.baseRange;
                        lightningOrb.damageType = this.damageType;
                        lightningOrb.orbEffectPrefab = this.orbEffectPrefab;
                        OrbManager.instance.AddOrb(lightningOrb);
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
            if (currentVictim && currentVictim.body)
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
