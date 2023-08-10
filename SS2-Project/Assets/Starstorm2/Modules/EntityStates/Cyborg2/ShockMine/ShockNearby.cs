using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using Moonstorm.Starstorm2.Components;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.Networking;
using Moonstorm.Starstorm2;
namespace EntityStates.Cyborg2.ShockMine
{
    public class ShockNearby : BaseShockMineState
    {
        protected override bool shouldStick => false;

        public static int numShocks = 4;
        public static float baseDuration = 4f;
        public static float blastRadius = 10f;

        public static GameObject shockEffectPrefab = SS2Assets.LoadAsset<GameObject>("Cyborg2ShockNova", SS2Bundle.Indev); // im lazy stfu

        private ProjectileDamage projectileDamage;
        private float shockInterval;
        private float shockTimer;
        private int timesShocked;
        public override void OnEnter()
        {
            base.OnEnter();

            this.projectileDamage = base.GetComponent<ProjectileDamage>();
            this.shockInterval = numShocks / baseDuration;
        }


        public override void FixedUpdate()
        {
            base.FixedUpdate();

            base.rigidbody.velocity = Vector3.zero;

            this.shockTimer -= Time.fixedDeltaTime;
            if(this.shockTimer <= 0)
            {
                this.shockTimer += this.shockInterval;
                this.Shock();
                this.timesShocked++;
            }

            if(this.timesShocked >= numShocks)
            {
                this.outer.SetNextState(new Expire());
                return;
            }
        }

        private void Shock()
        {
            if (!NetworkServer.active)
                return;

            if (shockEffectPrefab)
            {
                EffectManager.SpawnEffect(shockEffectPrefab, new EffectData
                {
                    origin = base.transform.position,
                    scale = blastRadius
                }, true);
            }
            BlastAttack blastAttack = new BlastAttack();
            blastAttack.position = base.transform.position;
            blastAttack.baseDamage = this.projectileDamage.damage;
            blastAttack.baseForce = 0f;
            blastAttack.radius = blastRadius;
            blastAttack.attacker = (this.projectileController.owner ? this.projectileController.owner.gameObject : null);
            blastAttack.inflictor = base.gameObject;
            blastAttack.teamIndex = this.projectileController.teamFilter.teamIndex;
            blastAttack.crit = this.projectileDamage.crit;
            blastAttack.procChainMask = this.projectileController.procChainMask;
            blastAttack.procCoefficient = this.projectileController.procCoefficient;
            blastAttack.bonusForce = Vector3.zero;
            blastAttack.falloffModel = BlastAttack.FalloffModel.None;
            blastAttack.damageColorIndex = this.projectileDamage.damageColorIndex;
            blastAttack.damageType = this.projectileDamage.damageType;
            blastAttack.attackerFiltering = AttackerFiltering.Default;
            blastAttack.Fire();
        }
    }
}
