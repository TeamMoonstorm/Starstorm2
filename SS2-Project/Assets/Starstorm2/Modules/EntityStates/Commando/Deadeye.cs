using EntityStates;
using EntityStates.Commando.CommandoWeapon;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Starstorm2.Modules.EntityStates.Commando
{
    public class Deadeye : BaseSkillState
    {
        [SerializeField]
        public static float damageCoeff = 1.65f;

        [SerializeField]
        public static float procCoeff = 0.7f;

        public float baseDuration = 0.5f;
        private float duration;

        public GameObject hitEffectPrefab = FireBarrage.hitEffectPrefab;
        public GameObject tracerEffectPrefab = FireBarrage.tracerEffectPrefab;

        private int pistolSide = 1;

        private void PlayPistolAnimation()
        {
            if (pistolSide == 1)
            {
                pistolSide = 2;
                base.PlayAnimation("Gesture Additive, Right", "FirePistol, Right");
                if (FireBarrage.effectPrefab)
                {
                    EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, base.gameObject, "MuzzleRight", false);
                }
            } 
            else if (pistolSide == 2)
            {
                pistolSide = 1;
                base.PlayAnimation("Gesture Additive, Left", "FirePistol, Left");
                if (FireBarrage.effectPrefab)
                {
                    EffectManager.SimpleMuzzleFlash(FireBarrage.effectPrefab, base.gameObject, "MuzzleLeft", false);
                }
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();
            this.duration = this.baseDuration / base.attackSpeedStat;
            Ray aimRay = base.GetAimRay();
            base.StartAimMode(aimRay, 2f, false);

            PlayPistolAnimation();
            Util.PlaySound(FireBarrage.fireBarrageSoundString, base.gameObject);
            base.AddRecoil(-0.6f, 0.6f, -0.6f, 0.6f);

            var isCrit = base.RollCrit();

            if (base.isAuthority)
            {

                if (isCrit)
                {
                    // Do the thing swuff mentioned
                }

                var bullet = new BulletAttack
                {
                    owner = base.gameObject,
                    weapon = base.gameObject,
                    origin = aimRay.origin,
                    aimVector = aimRay.direction,
                    minSpread = 0f,
                    maxSpread = base.characterBody.spreadBloomAngle,
                    bulletCount = 1U,
                    procCoefficient = procCoeff,
                    damage = base.characterBody.damage * damageCoeff,
                    force = 3,
                    falloffModel = BulletAttack.FalloffModel.None,
                    tracerEffectPrefab = this.tracerEffectPrefab,
                    muzzleName = "MuzzleRight",
                    hitEffectPrefab = this.hitEffectPrefab,
                    isCrit = isCrit,
                    HitEffectNormal = false,
                    stopperMask = LayerIndex.world.mask,
                    smartCollision = true,
                    maxDistance = 300f
                };

                bullet.Fire();
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
