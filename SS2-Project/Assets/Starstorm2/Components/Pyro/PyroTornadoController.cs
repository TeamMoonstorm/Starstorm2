using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Projectile;
using SS2;
using TMPro;

[RequireComponent(typeof(ProjectileController))]
public class PyroTornadoController : NetworkBehaviour
{
    private List<CharacterBody> processedVictims = new List<CharacterBody>();
    private ProjectileDamage projectileDamage;
    private ProjectileController projectileController;

    public Transform rangeIndicator;
    public GameObject delayBlastPrefab;

    [Header("Effect Prefabs")]
    public GameObject impactEffectPrefab;
    public GameObject indicatorExplosionEffectPrefab;

    public static float damageCoefficient = 8f;
    public static float force = 300f;

    public static float baseRadius = 10f;
    public static float maxRadiusPerIgnite = 4f; // TODO: volume increase instead of radius?
    public static float minRadiusPerIgnite = 1f;
    public static int maxIgniteHits = 4;

    public static float diffForMaxGrowthSpeed = 9f;
    public static float maxGrowthSpeed = 64f; // radius increase in meters per second
    public static float minGrowthSpeed = 1f;
    public static float decayDuration = 1.2f;
    public static float decayDurationPerIgnite = 0.25f;

    public static float searchInterval = 0.125f;

    public static float delayBlastDamageCoefficient = 4f;
    public static float delayBlastForce = 1000f;
    public static float delayBlastRadius = 11f;
    public static float delayBlastVictimRadiusCoefficient = 1f;
    public static float delayBlastProcCoefficient = 1f;
    public static float delayBlastTimer = 1f;

    private float targetRadius;
    private float currentRadius;
    private int igniteHits;

    private AnimateShaderAlpha[] animateShaderAlphas;
    private SphereSearch sphereSearch;
    private HurtBox[] hits;
    private float stopwatch;
    private float lifetimeStopwatch;

    public void Start()
    {
        hits = new HurtBox[100];
        targetRadius = baseRadius;

        projectileController = GetComponent<ProjectileController>();
        projectileDamage = GetComponent<ProjectileDamage>();

        if (NetworkServer.active)
        {
            sphereSearch = new SphereSearch();
        }

        if (rangeIndicator)
        {
            animateShaderAlphas = rangeIndicator.GetComponentsInChildren<AnimateShaderAlpha>();
            if (animateShaderAlphas != null)
            {
                for (int i = 0; i < animateShaderAlphas.Length; i++)
                {
                    animateShaderAlphas[i].timeMax = decayDuration;
                }
            }

            rangeIndicator.localScale = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        CalculateRadius(Time.fixedDeltaTime);

        if (NetworkServer.active)
        {
            stopwatch -= Time.fixedDeltaTime;
            if (stopwatch <= 0)
            {
                stopwatch += searchInterval;
                SearchForTargets();
            }

            lifetimeStopwatch += Time.fixedDeltaTime;
            if (lifetimeStopwatch >= decayDuration)
            {
                Destroy(gameObject);
            }
        }
    }

    private void CalculateRadius(float deltaTime)
    {
        float diff = targetRadius - currentRadius;
        float t = diff / diffForMaxGrowthSpeed;
        float growthSpeed = Mathf.Lerp(minGrowthSpeed, maxGrowthSpeed, t); // further from target = faster growth
        float growth = growthSpeed * deltaTime;
        growth = Mathf.Min(growth, diff); // radius to add this frame. don't exceed targetRadius
        currentRadius += growth;

        if (rangeIndicator)
        {
            rangeIndicator.localScale = Vector3.one * currentRadius;
        }
    }

    private void SearchForTargets()
    {
        sphereSearch.mask = LayerIndex.entityPrecise.mask;
        sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
        sphereSearch.origin = transform.position;
        sphereSearch.radius = currentRadius;
        var mask = TeamMask.GetUnprotectedTeams(projectileController.teamFilter.teamIndex);
        hits = sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).OrderCandidatesByDistance().GetHurtBoxes();
        for (int i = 0; i < hits.Length; i++)
        {
            TryProcessVictim(hits[i]);
        }
    }

    public void TryProcessVictim(HurtBox victim)
    {
        if (NetworkServer.active)
        {
            // we have a victim body and haven't already checked them out:
            if (victim != null && victim.healthComponent != null && !processedVictims.Contains(victim.healthComponent.body))
            {
                CharacterBody victimBody = victim.healthComponent.body;
                processedVictims.Add(victimBody);

                PerformDamageServer(victim);

                if (impactEffectPrefab)
                {
                    EffectManager.SimpleEffect(impactEffectPrefab, victim.transform.position, Quaternion.identity, true);
                }
            }
        }
    }

    private void PerformDamageServer(HurtBox hurtBox)
    {
        if (!hurtBox || !hurtBox.healthComponent)
        {
            return;
        }
        HealthComponent healthComponent = hurtBox.healthComponent;
        DamageInfo damageInfo = new DamageInfo();
        damageInfo.attacker = projectileController.owner;
        damageInfo.procChainMask = projectileController.procChainMask;
        damageInfo.procCoefficient = projectileController.procCoefficient;
        damageInfo.inflictor = gameObject;
        damageInfo.damage = projectileDamage.damage * damageCoefficient;
        damageInfo.crit = projectileDamage.crit;
        damageInfo.force = (hurtBox.transform.position - transform.position).normalized * force;
        damageInfo.damageType = projectileDamage.damageType;
        damageInfo.damageColorIndex = projectileDamage.damageColorIndex;
        damageInfo.position = hurtBox.transform.position;
        damageInfo.canRejectForce = true;
        damageInfo.inflictedHurtbox = hurtBox;
        healthComponent.TakeDamage(damageInfo);
        GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
        GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);

        if (!damageInfo.rejected)
        {
            var victimBody = healthComponent.body;
            int burnCount = victimBody.GetBurnCountPyro();
            if (burnCount > 0)
            {
                RpcOnIgnitedEnemyHit(hurtBox.transform.position);

                Vector3 corePosition = victimBody.corePosition;
                GameObject delayBlastObject = GameObject.Instantiate<GameObject>(delayBlastPrefab, corePosition, Quaternion.identity);
                DelayBlast delayBlast = delayBlastObject.GetComponent<DelayBlast>();
                delayBlast.position = corePosition;
                delayBlast.baseDamage = projectileDamage.damage * delayBlastDamageCoefficient;
                delayBlast.procCoefficient = projectileController.procCoefficient * delayBlastProcCoefficient;
                delayBlast.procChainMask = projectileController.procChainMask;
                delayBlast.baseForce = delayBlastForce;
                delayBlast.radius = delayBlastRadius + victimBody.radius * delayBlastVictimRadiusCoefficient;
                delayBlast.attacker = projectileController.owner;
                delayBlast.inflictor = gameObject;
                delayBlast.crit = projectileDamage.crit;
                delayBlast.maxTimer = delayBlastTimer;
                delayBlast.damageColorIndex = projectileDamage.damageColorIndex;
                delayBlast.falloffModel = BlastAttack.FalloffModel.Linear;
                delayBlastObject.GetComponent<TeamFilter>().teamIndex = projectileController.teamFilter.teamIndex;
                NetworkServer.Spawn(gameObject);
            }
        }
    }

    [ClientRpc]
    public void RpcOnIgnitedEnemyHit(Vector3 hitPosition)
    {
        float t = (float)igniteHits / (float)maxIgniteHits;
        float radiusIncrease = Mathf.Lerp(maxRadiusPerIgnite, minRadiusPerIgnite, t);
        targetRadius += radiusIncrease;
        igniteHits++;

        lifetimeStopwatch -= decayDurationPerIgnite;

        if (animateShaderAlphas != null)
        {
            for (int i = 0; i < animateShaderAlphas.Length; i++)
            {
                animateShaderAlphas[i].time -= decayDurationPerIgnite;
            }
        }

        if (rangeIndicator)
        {
            if (indicatorExplosionEffectPrefab)
            {
                Vector3 between = hitPosition - transform.position;
                GameObject effectInstance = GameObject.Instantiate(indicatorExplosionEffectPrefab, rangeIndicator);
                effectInstance.transform.forward = between.normalized;
            }
        }
    }
}
