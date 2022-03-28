/*using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Borg
{
    public class BorgFireBFG : BaseSkillState
    {
        public float damageCoefficient = 1.85f;
        public float baseDuration = 0.5f;
        public float recoil = 1f;

        private float duration;
        private float fireDuration;
        private bool hasFired;
        private Animator animator;
        private string muzzleString;

        private float groundKnockbackDistance = 8;
        private float airKnockbackDistance = 12;



        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            fireDuration = 0.25f * duration;
            characterBody.SetAimTimer(2f);
            animator = GetModelAnimator();
            muzzleString = "Muzzle";


            PlayAnimation("Gesture, Override", "FireSpecial", "FireArrow.playbackRate", duration);

            if (isAuthority && characterBody && characterBody.characterMotor)
            {
                float height = characterBody.characterMotor.isGrounded ? groundKnockbackDistance : airKnockbackDistance;
                float num3 = characterBody.characterMotor ? characterBody.characterMotor.mass : 1f;
                float acceleration2 = characterBody.acceleration;
                float num4 = Trajectory.CalculateInitialYSpeedForHeight(height, -acceleration2);
                characterBody.characterMotor.ApplyForce(-num4 * num3 * GetAimRay().direction, false, false);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
        }

        private void FireBFG()
        {
            if (!hasFired)
            {
                string soundString = "BorgUtility";//base.effectComponent.shootSound;
                //if (isCrit) soundString += "Crit";
                Util.PlaySound(soundString, gameObject);
                hasFired = true;

                characterBody.AddSpreadBloom(0.75f);
                Ray aimRay = GetAimRay();
                EffectManager.SimpleMuzzleFlash(Commando.CommandoWeapon.FirePistol2.muzzleEffectPrefab, gameObject, muzzleString, false);

                if (isAuthority)
                {
                    ProjectileManager.instance.FireProjectile(Starstorm2.Survivors.Borg.bfgProjectile,
                        aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction),
                        gameObject, characterBody.damage * 1f,
                        0f,
                        Util.CheckRoll(characterBody.crit,
                        characterBody.master),
                        DamageColorIndex.Default, null, -1f);
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (fixedAge >= fireDuration)
            {
                FireBFG();
            }

            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}*/