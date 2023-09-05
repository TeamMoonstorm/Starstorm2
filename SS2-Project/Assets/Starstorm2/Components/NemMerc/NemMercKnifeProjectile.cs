using System;
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
        public float pullTimer = 0.2f;

        private GameObject owner;
        private EntityStateMachine ownerBodyMachine;
        private bool alive = false;

        public GameObject pullEffectPrefab;

        private void Awake()
        {
            this.projectileStickOnImpact = base.GetComponent<ProjectileStickOnImpact>();
        }
        private void Start()
        {
            this.owner = base.GetComponent<ProjectileController>().owner;
            this.ownerBodyMachine = EntityStateMachine.FindByCustomName(this.owner, "Body");
        }

        private void FixedUpdate()
        {
            if (this.alive) this.pullTimer -= Time.fixedDeltaTime;
            else this.pullTimer = 0.2f;

            if (this.projectileStickOnImpact)
            {
                this.alive = this.projectileStickOnImpact.stuckBody;
            }

            if (this.alive && this.pullTimer <= 0f && this.owner)
            {
                float distanceBetween = (this.owner.transform.position - base.transform.position).magnitude;
                if (distanceBetween <= this.pullRadius)
                {
                    this.alive = false;
                    this.PullKnife();
                }
            }

            
        }

        public void PullKnife()
        {
            EffectManager.SimpleEffect(this.pullEffectPrefab, base.transform.position, base.transform.rotation, false); //vfx
            Util.PlaySound("Play_gup_attack1_shoot", base.gameObject); //sound

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
                damageType = DamageType.Generic,
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
