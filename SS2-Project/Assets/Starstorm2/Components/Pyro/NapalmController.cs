using System;
using System.Collections.Generic;
using HG;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace SS2.Components
{
    public static class NapalmManager
    {
        public static void ReportHit(GameObject victim, GameObject attacker, float damageInterval)
        {
            var hitReport = new HitReport
            {
                victim = victim,
                attacker = attacker,
                ignoreTime = Run.FixedTimeStamp.now + damageInterval,
            };
            hitReports.Add(hitReport);
        }

        public static bool ShouldHitProceed(GameObject victim, GameObject attacker)
        {
            for (int i = 0; i < hitReports.Count; i++)
            {
                var hitReport = hitReports[i];
                if (hitReport.victim == victim && hitReport.attacker == attacker)
                {
                    if (hitReport.ignoreTime.hasPassed)
                    {
                        hitReports.RemoveAt(i);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        private static readonly List<HitReport> hitReports = new List<HitReport>();

        // each napalm instance does its own collision check on its own timer
        // if it hits an enemy, check with NapalmManager to see if that enemy has already been damaged "recently"
        // if it hasn't, then damage the enemy and give NapalmManager a HitReport.
        // then, that enemy will be ignored by all napalm until an amount of time has passed, equal to its damageInterval
        // this is on a per-attacker basis
        public struct HitReport
        {
            public Run.FixedTimeStamp ignoreTime;
            public GameObject victim;
            public GameObject attacker;
        }
    }

    [RequireComponent(typeof(ProjectileController))]
    [RequireComponent(typeof(ProjectileDamage))]
    [RequireComponent(typeof(HitBoxGroup))]
    [RequireComponent(typeof(ProjectileStickOnImpact))]
    public class NapalmController : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            RoR2Application.onUpdate += UpdateOptimizedDamageUpdateInterval;
        }
        // damagetrail does this so sure why not
        private static void UpdateOptimizedDamageUpdateInterval()
        {
            float b = 0.4f;
            float a = 0.2f;
            float sixtyFramesPerFuckingSecond = 60f;
            float currentFramesPerSecond = 1f / Time.deltaTime;
            if (currentFramesPerSecond > 50f)
            {
                NapalmController.optimizedDamageInterval = a;
                return;
            }
            float num3 = (sixtyFramesPerFuckingSecond - currentFramesPerSecond) / 30f;
            NapalmController.optimizedDamageInterval = Mathf.Lerp(minDamageInterval, maxDamageInterval, num3);
        }

        public Transform stuckEnemyEffectTransform;
        public Transform stuckWorldEffectTransform;

        public UnityEvent onEnd;

        public static float lifetime = 7f;
        public static float endTime = 0.5f;
        public static float damageCoefficientPerSecond = 2.12f; 
        public static float procCoefficientPerSecond = 3.6f;
        public static float minDamageInterval = 0.2f;
        public static float maxDamageInterval = 0.4f;
        public static float optimizedDamageInterval = 0.2f;

        private ProjectileController projectileController;
        private ProjectileDamage projectileDamage;
        private ProjectileStickOnImpact projectileStickOnImpact;
        private HitBoxGroup hitBoxGroup;

        private float ownerDamage; // huge fucking chain of states spawning projectiles spawning projectiles with a damagecoefficient at each step. no thanks!
        private float damageStopwatch;
        private float lifetimeStopwatch;
        private bool hasEnded = false;
        private void Awake()
        {
            projectileController = GetComponent<ProjectileController>();
            projectileDamage = GetComponent<ProjectileDamage>();
            projectileStickOnImpact = GetComponent<ProjectileStickOnImpact>();
            hitBoxGroup = GetComponent<HitBoxGroup>();

            UpdateEffects();
        }
        private void Start()
        {
            if (projectileController.owner && projectileController.owner.TryGetComponent(out CharacterBody body))
            {
                ownerDamage = body.damage;
            }

            UpdateEffects();
        }


        private void FixedUpdate()
        {
            UpdateEffects();

            lifetimeStopwatch += Time.fixedDeltaTime;

            if (!hasEnded && lifetimeStopwatch >= lifetime - endTime)
            {
                hasEnded = true;

                onEnd?.Invoke();
            }

            if (NetworkServer.active)
            {
                damageStopwatch += Time.fixedDeltaTime;
                if (damageStopwatch >= optimizedDamageInterval)
                {
                    damageStopwatch -= optimizedDamageInterval;
                    DoDamage(optimizedDamageInterval);
                }

                if (lifetimeStopwatch >= lifetime)
                {
                    Destroy(gameObject);
                }
            }
        }
        private void UpdateEffects()
        {
            if (projectileStickOnImpact.stuck && projectileStickOnImpact.NetworkhitHurtboxIndex >= 0)
            {
                if (stuckEnemyEffectTransform) stuckEnemyEffectTransform.gameObject.SetActive(true);
                if (stuckWorldEffectTransform) stuckWorldEffectTransform.gameObject.SetActive(false);
            }
            else
            {
                if (stuckEnemyEffectTransform) stuckEnemyEffectTransform.gameObject.SetActive(false);
                if (stuckWorldEffectTransform) stuckWorldEffectTransform.gameObject.SetActive(true);
            }
        }

        private void DoDamage(float damageInterval)
        {
            if (!NetworkServer.active)
            {
                return;
            }

            TeamIndex attackerTeamIndex = TeamIndex.Neutral;
            float damage = damageCoefficientPerSecond * damageInterval * ownerDamage;
            float procCoefficient = procCoefficientPerSecond * damageInterval;
            if (projectileController.owner)
            {
                attackerTeamIndex = projectileController.teamFilter.teamIndex;
            }
            DamageInfo damageInfo = new DamageInfo();
            damageInfo.attacker = projectileController.owner;
            damageInfo.inflictor = gameObject;
            damageInfo.crit = projectileDamage.crit;
            damageInfo.damage = damage;
            damageInfo.damageColorIndex = projectileDamage.damageColorIndex;
            damageInfo.damageType = DamageTypeCombo.GenericSecondary;
            R2API.DamageAPI.AddModdedDamageType(damageInfo, SS2.Survivors.Pyro.PyroIgniteOnHit);
            damageInfo.force = Vector3.zero;
            damageInfo.procCoefficient = procCoefficient;
            damageInfo.procChainMask = projectileController.procChainMask;

            for (int i = 0; i < hitBoxGroup.hitBoxes.Length; i++)
            {
                var hitBox = hitBoxGroup.hitBoxes[i];
                if (hitBox && hitBox.enabled && hitBox.gameObject && hitBox.gameObject.activeInHierarchy && hitBox.transform)
                {
                    Vector3 position = hitBox.transform.position;
                    Vector3 halfExtents = hitBox.transform.lossyScale * 0.5f;
                    Quaternion orientation = hitBox.transform.rotation;
                    Collider[] hits;
                    int numHits = HGPhysics.OverlapBox(out hits, position, halfExtents, orientation, LayerIndex.entityPrecise.mask, QueryTriggerInteraction.UseGlobal);
                    for (int j = 0; j < numHits; j++)
                    {
                        HurtBox hurtBox = hits[j].GetComponent<HurtBox>();
                        if (hurtBox)
                        {
                            HealthComponent healthComponent = hurtBox.healthComponent;
                            if (healthComponent)
                            {
                                if (FriendlyFireManager.ShouldSplashHitProceed(healthComponent, attackerTeamIndex))
                                {
                                    if (NapalmManager.ShouldHitProceed(healthComponent.gameObject, projectileController.owner))
                                    {
                                        damageInfo.position = hits[j].transform.position;
                                        damageInfo.inflictedHurtbox = hurtBox;

                                        healthComponent.TakeDamage(damageInfo);
                                        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
                                        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);

                                        NapalmManager.ReportHit(healthComponent.gameObject, projectileController.owner, damageInterval);
                                    }
                                    
                                }
                            }
                        }
                    }
                    HGPhysics.ReturnResults(hits);
                }
            }
        }
    }
}
