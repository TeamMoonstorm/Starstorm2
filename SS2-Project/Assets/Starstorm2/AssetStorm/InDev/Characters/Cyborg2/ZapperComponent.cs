using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2.Projectile;
using RoR2;
using RoR2.Orbs;

namespace Moonstorm.Starstorm2.Components
{
    public class ZapperComponent : MonoBehaviour, IProjectileImpactBehavior
    {
        public string chargeSound = "Play_railgunner_shift_chargeUp";
        public float chargeTime = 1f;
        public float orbDuration = 0.5f;
        public bool charging;
        private float chargeStopwatch;
        void Start()
        {

        }
        public void StartCharge()
        {
            this.charging = true;
            this.chargeStopwatch = this.chargeTime;
        }

        private void FixedUpdate()
        {
            this.chargeStopwatch -= Time.fixedDeltaTime;

            if (this.charging && this.chargeStopwatch <= 0)
            {
                this.FireOrb();
            }
        }

        public void FireOrb()
        {
            GameObject owner = base.GetComponent<ProjectileController>().owner;
            if (!owner) return;
            CharacterBody body = owner.GetComponent<CharacterBody>();
            if (!body) return;

            HurtBox target = body.mainHurtBox;

            Orb orb = new CyborgEnergyBuffOrb
            {
                duration = this.orbDuration,
                origin = base.transform.position,
                target = target,
            };
            OrbManager.instance.AddOrb(orb);

            Destroy(this);
            //vfx
            //sound
        }

        // wont fucking destory on world for some reason
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            Collider collider = impactInfo.collider;
            if (collider)
            {
                HurtBox component = collider.GetComponent<HurtBox>();
                if (!component)
                {
                    Destroy(base.gameObject);
                }
            }
        }
    }
}

