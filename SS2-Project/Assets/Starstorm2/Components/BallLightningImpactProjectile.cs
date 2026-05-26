using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using RoR2;
using RoR2.Projectile;
using SS2;
using TMPro;
using RoR2.Orbs;

[RequireComponent(typeof(ProjectileController))]
public class BallLightningImpactProjectile : NetworkBehaviour
{
    private List<CharacterBody> processedHurtboxes = new List<CharacterBody>();
    private ProjectileDamage projectileDamage;
    private ProjectileController projectileController;

    [Header("Effect Prefabs")]
    public GameObject impactEffectPrefab;
    public GameObject orbEffectPrefab;
    public GameObject orbImpactEffectPrefab;
    public GameObject gigaOrbEffectPrefab;
    public GameObject gigaOrbImpactEffectPrefab;

    public static float stormEliteDamageMultiplier = 3f;
    public static float orbDamageCoefficient = 20f;
    public static float orbProcCoefficient = 1f;
    public static float orbBlastRadius = 3f;

    public static float orbDuration = 0.5f;
    public static float blastRadius = 24f;

    public static float diffForMaxGrowthSpeed = 9f;
    public static float maxGrowthSpeed = 64f; // radius increase in meters per second
    public static float minGrowthSpeed = 1f;
    public static float lifetime = 1.2f;

    public static float searchInterval = 0.125f;

    private float targetRadius;
    private float currentRadius;

    private SphereSearch sphereSearch;
    private HurtBox[] hits;
    private float stopwatch;
    private float lifetimeStopwatch;

    public void Start()
    {
        hits = new HurtBox[100];
        targetRadius = blastRadius;

        projectileController = GetComponent<ProjectileController>();
        projectileDamage = GetComponent<ProjectileDamage>();

        if (NetworkServer.active)
        {
            sphereSearch = new SphereSearch();
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
            if (lifetimeStopwatch >= lifetime)
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
        currentRadius = Mathf.Min(currentRadius + growth, blastRadius);
    }


    private static float teleporterRadius = 7f;
    private void SearchForTargets()
    {
        float sqrRadius = currentRadius * currentRadius;

        // search for enemies
        sphereSearch.mask = LayerIndex.entityPrecise.mask;
        sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
        sphereSearch.origin = transform.position;
        sphereSearch.radius = currentRadius;
        var mask = TeamMask.GetUnprotectedTeams(projectileController.teamFilter.teamIndex);
        hits = sphereSearch.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).OrderCandidatesByDistance().GetHurtBoxes();
        for (int i = 0; i < hits.Length; i++)
        {
            TryProcessHurtbox(hits[i]);
        }

        // search for chests

        // search for teleporter
        if (TeleporterInteraction.instance)
        {
            Vector3 between = transform.position - TeleporterInteraction.instance.transform.position;
            if (between.sqrMagnitude < sqrRadius + teleporterRadius * teleporterRadius) // distance to teleporter edge, not center
            {
                ProcessTeleporter(TeleporterInteraction.instance);
            }
        }
    }

    public void TryProcessHurtbox(HurtBox victim)
    {
        if (NetworkServer.active)
        {
            // we have a victim body and haven't already checked them out:
            if (victim != null && victim.healthComponent != null && !processedHurtboxes.Contains(victim.healthComponent.body))
            {
                CharacterBody victimBody = victim.healthComponent.body;
                processedHurtboxes.Add(victimBody);

                PerformDamageServer(victim.hurtBoxGroup ? victim.hurtBoxGroup.mainHurtBox : victim);

                if (impactEffectPrefab)
                {
                    EffectManager.SimpleEffect(impactEffectPrefab, victim.transform.position, Quaternion.identity, true);
                }
            }
        }
    }

    public void ProcessTeleporter(TeleporterInteraction teleporter)
    {
        if (TeleporterUpgradeController.instance)
        {
            TeleporterUpgradeController.instance.UpgradeStorm(true); // TODO: MAKE THIS HSIT WORK !!!!! AHHHHHHHHHHHHHHHHHH
        }
    }

    private void PerformDamageServer(HurtBox hurtBox)
    {
        if (!hurtBox || !hurtBox.healthComponent)
        {
            return;
        }

        float damage = projectileDamage.damage * orbDamageCoefficient;
        bool isGiga = hurtBox.healthComponent.body.HasBuff(SS2Content.Buffs.BuffAffixStorm);
        if (isGiga)
        {
            damage *= stormEliteDamageMultiplier;
        }

        OrbManager.instance.AddOrb(new BallLightningOrb
        {
            attacker = projectileController.owner,
            damageColorIndex = DamageColorIndex.Electrocution,
            teamIndex = projectileController.teamFilter.teamIndex,
            damageValue = damage,
            isCrit = projectileDamage.crit,
            procChainMask = projectileController.procChainMask,
            damageType = DamageType.Shock5s,
            procCoefficient = orbProcCoefficient,
            target = hurtBox,

            giga = isGiga,
            inflictor = gameObject,
            orbEffectPrefab = isGiga ? gigaOrbEffectPrefab : orbEffectPrefab,
            orbImpactEffectPrefab = isGiga ? gigaOrbImpactEffectPrefab : orbImpactEffectPrefab,
            blastRadius = orbBlastRadius,
        });

    }

    // TODO: unify the stupid storm lightning strike orbs
    public class BallLightningOrb : GenericDamageOrb, IOrbFixedUpdateBehavior
    {
        private Vector3 lastKnownTargetPosition;
        public float blastRadius = 3f;
        public GameObject inflictor;
        public GameObject orbEffectPrefab;
        public GameObject orbImpactEffectPrefab;

        public bool giga;

        public override void Begin()
        {
            base.Begin();
            base.duration = orbDuration;
        }

        public void FixedUpdate()
        {
            if (this.target)
            {
                this.lastKnownTargetPosition = this.target.transform.position;
            }
        }

        public override GameObject GetOrbEffect()
        {
            return orbEffectPrefab ?? LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/OrbEffects/LightningStrikeOrbEffect");
        }

        public override void OnArrival()
        {
            var effectPrefab = orbImpactEffectPrefab ?? LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ImpactEffects/LightningStrikeImpact");

            EffectManager.SpawnEffect(effectPrefab, new EffectData
            {
                origin = this.lastKnownTargetPosition
            }, true);

            new BlastAttack
            {
                attacker = this.attacker,
                baseDamage = this.damageValue,
                baseForce = 0f,
                bonusForce = Vector3.down * 3000f,
                crit = this.isCrit,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Shock5s,
                falloffModel = BlastAttack.FalloffModel.Linear,
                inflictor = this.inflictor,
                position = this.lastKnownTargetPosition,
                procChainMask = this.procChainMask,
                procCoefficient = procCoefficient,
                radius = blastRadius,
                teamIndex = this.teamIndex,
                attackerFiltering = AttackerFiltering.Default,
            }.Fire();

        }

    }
}
