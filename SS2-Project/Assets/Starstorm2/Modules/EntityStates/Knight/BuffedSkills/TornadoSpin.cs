using EntityStates;
using EntityStates.Knight;
using MSU;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace Assets.Starstorm2.Modules.EntityStates.Knight.BuffedSkills
{
    class TornadoSpin : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [FormatToken("SS2_KNIGHT_SPECIAL_SPIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;
        public static GameObject buffWard;
        public static float hopVelocity;
        public static float airControl;
        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float minimumY;
        public static float aimVelocity;
        public static GameObject beamProjectile;
        public static SkillDef originalSkillRef;

        private bool hasSpun;
        private int _origLayer;

        public override void OnEnter()
        {
            base.OnEnter();
            if (characterMotor)
            {
                _origLayer = characterMotor.capsuleCollider.gameObject.layer;
                characterMotor.capsuleCollider.gameObject.layer = LayerIndex.fakeActor.intVal;
                characterMotor.Motor.RebuildCollidableLayers();
            }

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

            if (animator.GetFloat("Utility") >= 0.5f && !hasSpun)
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

            if (base.isAuthority)
            {
                ProjectileManager.instance.FireProjectile(
                    beamProjectile,
                    GetAimRay().origin,
                    Util.QuaternionSafeLookRotation(GetAimRay().direction),
                    gameObject,
                    damageStat * damageCoefficient,
                    0f,
                    RollCrit(),
                    DamageColorIndex.Default,
                    null,
                    80f
                );
            }

            if (base.isAuthority)
            {
                GenericSkill primarySkill = skillLocator.primary;
                GenericSkill utilitySkill = skillLocator.utility;
                GenericSkill specialSkill = skillLocator.special;

                primarySkill.UnsetSkillOverride(gameObject, SwingSword.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
                utilitySkill.UnsetSkillOverride(gameObject, SpinUtility.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
                specialSkill.UnsetSkillOverride(gameObject, BannerSpecial.buffedSkillRef, GenericSkill.SkillOverridePriority.Contextual);
            }

            if (characterMotor) //Nasty fucking hack
            {
                characterMotor.capsuleCollider.gameObject.layer = _origLayer;
                characterMotor.Motor.RebuildCollidableLayers();
            }

            outer.SetNextStateToMain();
            base.OnExit();
        }

        public override void PlayAnimation()
        {
            PlayCrossfade("FullBody, Override", "Utility", "Utility.playbackRate", duration * swingTimeCoefficient, 0.15f);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}