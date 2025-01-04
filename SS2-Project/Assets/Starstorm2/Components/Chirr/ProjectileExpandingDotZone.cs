﻿using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using System;
namespace SS2.Components
{
    [RequireComponent(typeof(ProjectileController))]
    public class ProjectileExpandingDotZone : MonoBehaviour
    {
        public float startRadius = 7f;
        public float endRadius = 14f;
        public float fireFrequency = 3f;

        public float damageCoefficientPerSecond = 1f;
        public float procCoefficientPerSecond = 1f;

        public float lifetime = 5f;
        private float lifeStopwatch;

        private float fireStopwatch;

        private ProjectileController projectileController;
        private TeamFilter teamFilter;
        private ProjectileDamage projectileDamage;
        public Transform[] scaledTransforms;

        private float currentRadius;
        public GameObject impactEffect;
        public BuffDef allyBuff;
        public float allyBuffDuration = 3f;
        public BuffDef enemyBuff;
        public float enemyBuffDuration = 3f;

        [HideInInspector]
        public DamageAPI.ModdedDamageType moddedDamageType = (DamageAPI.ModdedDamageType)(-1);
        private void Awake()
        {
            this.projectileController = base.GetComponent<ProjectileController>();
            this.teamFilter = base.GetComponent<TeamFilter>();
            this.projectileDamage = base.GetComponent<ProjectileDamage>();
            this.currentRadius = startRadius;
            this.fireStopwatch = 1 / this.fireFrequency;
        }

        private bool ShouldGrantAllyBuff(TeamIndex teamIndex)
        {
            return this.teamFilter.teamIndex == teamIndex;
        }
        private bool ShouldGrantEnemyBuff(TeamIndex teamIndex)
        {
            return this.teamFilter.teamIndex != teamIndex;
        }
        private void FixedUpdate()
        {
            this.lifeStopwatch += Time.fixedDeltaTime;

            this.currentRadius = Mathf.Lerp(startRadius, endRadius, this.lifeStopwatch / this.lifetime);
            foreach (Transform transform in scaledTransforms)
            {
                transform.localScale = Vector3.one * this.currentRadius;
            }
            if(this.lifeStopwatch >= this.lifetime)
            {
                Destroy(base.gameObject);
                return;
            }
            this.fireStopwatch -= Time.fixedDeltaTime;
            if(this.fireStopwatch <= 0)
            {
                this.fireStopwatch += 1 / fireFrequency;
                this.Fire();
            }
        }

        private void Fire()
        {
            if (!NetworkServer.active) return;

            if (this.projectileDamage)
            {
                BlastAttack blastAttack = new BlastAttack();
                blastAttack.position = base.transform.position;
                blastAttack.baseDamage = this.projectileDamage.damage * this.damageCoefficientPerSecond / fireFrequency;
                blastAttack.baseForce = 0f;
                blastAttack.radius = this.currentRadius;
                blastAttack.attacker = this.projectileController.owner;
                blastAttack.inflictor = base.gameObject;
                blastAttack.teamIndex = this.projectileController.teamFilter.teamIndex;
                blastAttack.crit = this.projectileDamage.crit;
                blastAttack.procChainMask = this.projectileController.procChainMask;
                blastAttack.procCoefficient = this.projectileController.procCoefficient * this.procCoefficientPerSecond / fireFrequency;
                blastAttack.falloffModel = BlastAttack.FalloffModel.None;
                blastAttack.damageColorIndex = this.projectileDamage.damageColorIndex;
                blastAttack.damageType = this.projectileDamage.damageType;
                blastAttack.attackerFiltering = AttackerFiltering.Default;
                blastAttack.losType = BlastAttack.LoSType.None;
                blastAttack.impactEffect = EffectCatalog.FindEffectIndexFromPrefab(impactEffect);
                if (moddedDamageType != (DamageAPI.ModdedDamageType)(-1)) blastAttack.AddModdedDamageType(moddedDamageType);
                BlastAttack.Result result = blastAttack.Fire();
            }
            if (!allyBuff && !enemyBuff) return;
            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.radius = this.startRadius;
            sphereSearch.origin = base.transform.position;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
            for (int i = 0; i < hurtBoxes.Length; i++)
            {
                HealthComponent healthComponent = hurtBoxes[i].healthComponent;
                if (allyBuff && ShouldGrantAllyBuff(healthComponent.body.teamComponent.teamIndex))
                {
                    healthComponent.body.AddTimedBuff(allyBuff, 3f);
                }
                if(enemyBuff && ShouldGrantEnemyBuff(healthComponent.body.teamComponent.teamIndex))
                {
                    healthComponent.body.AddTimedBuff(enemyBuff, 3f);
                }
            }
        }
    }
}
