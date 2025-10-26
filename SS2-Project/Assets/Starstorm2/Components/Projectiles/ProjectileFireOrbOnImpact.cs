using System;
using System.Collections.Generic;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.Orbs;

namespace SS2
{
    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(ProjectileDamage))]
    public class ProjectileFireOrbOnImpact : MonoBehaviour, IProjectileImpactBehavior
    {
        [Header("Impact Properties")]
        public bool destroyOnWorld;
        public GameObject impactEffect;

        [Header("Orb Properties")]
        public float orbDuration = 0.125f;
        public int orbBounces = 2;
        public float orbRange = 12f;
        public float orbDamageCoefficient = 1f;
        public float orbDamageCoefficientPerBounce = 1f;
        public float orbProcCoefficient = 1f;
        public bool canProcErraticGadget = false;
        public GameObject orbEffectPrefab;

        private bool alive = true;
        private ProjectileController projectileController;
        private ProjectileDamage projectileDamage;

        private void Awake()
        {
            projectileController = GetComponent<ProjectileController>();
            projectileDamage = GetComponent<ProjectileDamage>();
        }

        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            if (!alive || !NetworkServer.active)
            {
                return;
            }
            // deal damage to hit target
            Collider collider = impactInfo.collider;
            HurtBox hitHurtBox;
            if (collider)
            {
                DamageInfo damageInfo = new DamageInfo();
                if (projectileDamage)
                {
                    damageInfo.damage = projectileDamage.damage;
                    damageInfo.crit = projectileDamage.crit;
                    damageInfo.attacker = projectileController.owner;
                    damageInfo.inflictor = base.gameObject;
                    damageInfo.position = impactInfo.estimatedPointOfImpact;
                    damageInfo.force = projectileDamage.force * base.transform.forward;
                    damageInfo.procChainMask = projectileController.procChainMask;
                    damageInfo.procCoefficient = projectileController.procCoefficient;
                    damageInfo.damageColorIndex = projectileDamage.damageColorIndex;
                    damageInfo.damageType = projectileDamage.damageType;
                }
                hitHurtBox = collider.GetComponent<HurtBox>();
                if (hitHurtBox)
                {
                    HealthComponent healthComponent = hitHurtBox.healthComponent;
                    if (healthComponent)
                    {
                        if (healthComponent.gameObject == projectileController.owner)
                        {
                            return;
                        }
                        if (FriendlyFireManager.ShouldDirectHitProceed(healthComponent, projectileController.teamFilter.teamIndex))
                        {
                            if (NetworkServer.active)
                            {
                                damageInfo.ModifyDamageInfo(hitHurtBox.damageModifier);
                                healthComponent.TakeDamage(damageInfo);
                                GlobalEventManager.instance.OnHitEnemy(damageInfo, hitHurtBox.healthComponent.gameObject);
                            }
                        }
                        alive = false;
                    }
                }
                else if (destroyOnWorld)
                {
                    alive = false;
                }
                damageInfo.position = base.transform.position;
                GlobalEventManager.instance.OnHitAll(damageInfo, collider.gameObject);

                // fire orb on impact
                if (!alive)
                {
                    if (impactEffect)
                    {
                        EffectManager.SimpleImpactEffect(impactEffect, impactInfo.estimatedPointOfImpact, -transform.forward, true);
                    }

                    CustomLightningOrb orb = new CustomLightningOrb();
                    orb.orbEffectPrefab = orbEffectPrefab;

                    orb.duration = orbDuration;
                    orb.bouncesRemaining = orbBounces - 1;
                    orb.range = orbRange;
                    orb.damageCoefficientPerBounce = orbDamageCoefficientPerBounce;
                    orb.damageValue = orbDamageCoefficient * projectileDamage.damage;
                    orb.damageType = DamageType.Generic;
                    orb.isCrit = projectileDamage.crit;
                    orb.damageColorIndex = DamageColorIndex.Default;
                    orb.procCoefficient = orbProcCoefficient * projectileController.procCoefficient;
                    orb.origin = transform.position;
                    orb.teamIndex = projectileController.teamFilter.teamIndex;
                    orb.canProcGadget = canProcErraticGadget;
                    orb.attacker = projectileController.owner;
                    orb.inflictor = base.gameObject;
                    orb.procChainMask = default(ProcChainMask);
                    orb.bouncedObjects = new List<HealthComponent> { };
                    if (hitHurtBox != null)
                    {
                        orb.bouncedObjects.Add(hitHurtBox.healthComponent);
                    }
                    HurtBox hurtbox = orb.PickNextTarget(base.transform.position, null);
                    if (hurtbox)
                    {
                        orb.target = hurtbox;
                        OrbManager.instance.AddOrb(orb);
                    }

                    Destroy(base.gameObject);
                }

            }
        }

    }
}
