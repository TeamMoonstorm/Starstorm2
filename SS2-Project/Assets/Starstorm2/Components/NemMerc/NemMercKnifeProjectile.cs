﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace Moonstorm.Starstorm2.Components
{
    [RequireComponent(typeof(ProjectileStickOnImpact))]
    class NemMercKnifeProjectile : MonoBehaviour
    {

        private ProjectileStickOnImpact projectileStickOnImpact;

        public float pullDamageCoefficient = 4f;
        public float pullProcCoefficient = 1f;
        public float pullRadius = 4.5f;
        public float pullTimer = 0.67f;

        private GameObject owner;
        private GameObject cloneOwner;
        private EntityStateMachine ownerBodyMachine;
        private TeamFilter teamFilter;
        private bool alive = false;

        public GameObject pullEffectPrefab;
        public ParticleSystem[] stickParticleSystem;
        public GameObject[] stickObject;
        private void Awake()
        {
            this.projectileStickOnImpact = base.GetComponent<ProjectileStickOnImpact>();
            this.teamFilter = base.GetComponent<TeamFilter>();
        }
        private void Start()
        {
            this.owner = base.GetComponent<ProjectileController>().owner;
            NemMercCloneTracker clone = this.owner.GetComponent<NemMercCloneTracker>();
            if(clone)
            {
                this.cloneOwner = clone.ownerTracker.gameObject;
            }
            this.ownerBodyMachine = EntityStateMachine.FindByCustomName(this.owner, "Body");
        }

        private void FixedUpdate()
        {
            if (this.alive) this.pullTimer -= Time.fixedDeltaTime;
            else this.pullTimer = 0.67f;

            if (this.projectileStickOnImpact)
            {
                bool newAlive = this.projectileStickOnImpact.stuckBody;
                if(newAlive && !this.alive)
                {
                    newAlive = TeamMask.GetEnemyTeams(this.teamFilter.teamIndex).HasTeam(this.projectileStickOnImpact.stuckBody.teamComponent.teamIndex);
                    if (newAlive)
                        OnEnemyBodyStuck();
                }
                else if(!newAlive && this.alive)
                {
                    this.OnEnemyBodyLost();
                }
                this.alive = newAlive;
            }

            if (this.alive && this.pullTimer <= 0f)
            {
                if(CheckPullKnife(this.owner) || CheckPullKnife(this.cloneOwner))
                {
                    this.alive = false;
                    this.PullKnife();
                }
            }

            
        }

        public void OnEnemyBodyStuck()
        {
            ParticleSystem[] array = this.stickParticleSystem;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Play();
            }
            GameObject[] array2 = this.stickObject;
            for (int i = 0; i < array2.Length; i++)
            {
                array2[i].SetActive(true);
            }
        }
        public void OnEnemyBodyLost()
        {
            ParticleSystem[] array = this.stickParticleSystem;
            for (int i = 0; i < array.Length; i++)
            {
                array[i].Stop();
            }
            GameObject[] array2 = this.stickObject;
            for (int i = 0; i < array2.Length; i++)
            {
                array2[i].SetActive(false);
            }
        }

        public bool CheckPullKnife(GameObject candidate)
        {
            if (!candidate) return false;
            float distanceBetween = (candidate.transform.position - base.transform.position).magnitude;
            return distanceBetween <= this.pullRadius;
        }

        public void PullKnife()
        {
            EffectManager.SimpleEffect(this.pullEffectPrefab, base.transform.position, base.transform.rotation, false); //vfx
            Util.PlaySound("Play_nemmerc_knife_rip", base.gameObject); //sound

            if (!NetworkServer.active) return;

            if (!this.projectileStickOnImpact) return;

            GameObject target = this.projectileStickOnImpact.victim;
            if (!target) return;

            HealthComponent healthComponent = target.GetComponent<HealthComponent>();
            if (!healthComponent) return;

            CharacterBody ownerBody = this.owner ? this.owner.GetComponent<CharacterBody>() : null;
            DamageInfo damageInfo = new DamageInfo
            {
                position = base.transform.position,
                attacker = this.owner,
                inflictor = this.owner,
                damage = this.pullDamageCoefficient * (ownerBody ? ownerBody.damage : 12f),
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.BonusToLowHealth,
                crit = ownerBody ? ownerBody.RollCrit() : false,
                force = Vector3.zero,
                procChainMask = default(ProcChainMask),
                procCoefficient = 1f
            };
            healthComponent.TakeDamage(damageInfo);
            GlobalEventManager.instance.OnHitEnemy(damageInfo, healthComponent.gameObject);
            GlobalEventManager.instance.OnHitAll(damageInfo, healthComponent.gameObject);

            Destroy(base.gameObject);
        }


        //projectiles are so fucking dogshit mannnnnnnnnnnnnnnnnnnnnnn
        //public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        //{
        //    if (!this.alive && !impactInfo.collider.GetComponent<HurtBox>())
        //        Destroy(base.gameObject);
        //}
    }
}