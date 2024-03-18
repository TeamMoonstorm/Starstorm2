using Moonstorm;
using Moonstorm.Starstorm2;
using Moonstorm.Starstorm2.DamageTypes;
using R2API;
using RoR2;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;

namespace EntityStates.Knight
{
    class SpinUtility : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [TokenModifier("SS2_KNIGHT_SPECIAL_SPIN_DESCRIPTION", StatTypes.MultiplyByN, 0, "100")]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;
        public static GameObject buffWard;
        public static SkillDef buffedSkillRef;
        public static float hopVelocity;
        private bool hasBuffed;
        private bool hasSpun;
        private GameObject wardInstance;


        public static float airControl;
        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float minimumY;
        public static float aimVelocity;


        public override void OnEnter()
        {
            base.OnEnter();
            hasBuffed = false;
            hasSpun = false;

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            animator = GetModelAnimator();

            Vector3 direction = GetAimRay().direction;

            // Launch Knight where they are aiming
            if (isAuthority)
            {
                characterBody.isSprinting = true;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 b = Vector3.up * upwardVelocity;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = a + b + b2;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (animator.GetFloat("SpecialSwing") >= 0.5f && !hasSpun)
            {
                hasSpun = true;
                if (!isGrounded)
                {
                    SmallHop(characterMotor, hopVelocity);
                }
            }
        }


        public override void OnExit()
        {
            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.OnExit();
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("Body", "SwingSpecial", "Special.playbackRate", duration * swingTimeCoefficient, 0.15f);   
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}