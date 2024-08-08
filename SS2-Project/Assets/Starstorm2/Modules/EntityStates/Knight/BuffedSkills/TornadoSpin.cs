using EntityStates;
using EntityStates.Knight;
using MSU;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace Assets.Starstorm2.Modules.EntityStates.Knight.BuffedSkills
{
    public class TornadoSpin : BasicMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [FormatToken("SS2_KNIGHT_SPECIAL_SPIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;

        public static GameObject beamProjectile;
        public static SkillDef originalSkillRef;

        private bool hasSpun;
        private int _origLayer;

        public float hopVelocity = new SpinUtility().hopVelocity + 10f;
        public float airControl = new SpinUtility().airControl + 1f;
        public float upwardVelocity = new SpinUtility().upwardVelocity + 1f;
        public float forwardVelocity = new SpinUtility().forwardVelocity + 1f;
        public float minimumY = new SpinUtility().minimumY + 1f;
        public float aimVelocity = new SpinUtility().aimVelocity + 1f;

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
            if (base.isAuthority)
            {
                base.characterBody.isSprinting = false;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 val = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 val2 = Vector3.up * upwardVelocity;
                Vector3 val3 = new Vector3(direction.x, 0f, direction.z);
                Vector3 val4 = val3.normalized * forwardVelocity;
                base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = val + val2 + val4;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(SS2.Survivors.Knight.dodgeFOV, 60f, base.fixedAge / duration);
            base.characterMotor.moveDirection = base.inputBank.moveVector;

            if (base.isAuthority && base.characterMotor.isGrounded)
            {
                this.outer.SetNextStateToMain();
                return;
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

                utilitySkill.DeductStock(1);
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