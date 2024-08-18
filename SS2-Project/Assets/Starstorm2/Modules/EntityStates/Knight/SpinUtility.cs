using MSU;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using UnityEngine;

namespace EntityStates.Knight
{
    class SpinUtility : BaseKnightMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [FormatToken("SS2_KNIGHT_SPECIAL_SPIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;
        public static SkillDef buffedSkillRef;
        public static float baseFireFrequency = 0.5f;

        private int _origLayer;

        // Movement variables
        public float hopVelocity = 30f;
        public float airControl = 2f;
        public float upwardVelocity = 4f;
        public float forwardVelocity = 20f;
        public float minimumY = 0.10f;
        public float aimVelocity = 3f;

        private float fireFrequency;
        private bool hitOverlapLastTick;
        private float fireAge;


        public override void OnEnter()
        {
            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            animator = GetModelAnimator();

            if (characterMotor)
            {
                _origLayer = characterMotor.capsuleCollider.gameObject.layer;
                characterMotor.capsuleCollider.gameObject.layer = LayerIndex.fakeActor.intVal;
                characterMotor.Motor.RebuildCollidableLayers();
            }

            // Launch Knight where they are aiming
            if (base.isAuthority)
            {
                Vector3 direction = GetAimRay().direction;
                base.characterBody.isSprinting = false;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 val = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 val2 = Vector3.up * upwardVelocity;
                Vector3 val3 = new Vector3(direction.x, 0f, direction.z).normalized;
                Vector3 val4 = val3.normalized * forwardVelocity;
                base.characterMotor.velocity = val + val2 + val4;
            }

            if (!isGrounded)
            {
                SmallHop(characterMotor, hopVelocity);
            }


            hitboxGroupName = "BigHitbox";

            damageType = DamageType.Generic;
            damageCoefficient = 12;
            procCoefficient = 1f;
            pushForce = 400f;
            bonusForce = Vector3.zero;
            baseDuration = 1f;

            //0-1 multiplier of baseduration, used to time when the hitbox is out (usually based on the run time of the animation)
            //for example, if attackStartPercentTime is 0.5, the attack will start hitting halfway through the ability. if baseduration is 3 seconds, the attack will start happening at 1.5 seconds
            attackStartPercentTime = 0.1f;
            attackEndPercentTime = 0.7f;

            //this is the point at which the attack can be interrupted by itself, continuing a combo
            earlyExitPercentTime = 0.5f;

            hitStopDuration = 0.012f;
            attackRecoil = 0.5f;
            hitHopVelocity = 9f;

            swingSoundString = "NemmandoSwing";
            hitSoundString = "";
            muzzleString = "SwingCenter";
            playbackRateParam = "Util.Hitbox";
            swingEffectPrefab = SS2.Survivors.Knight.KnightSpinEffect;
            hitEffectPrefab = SS2.Survivors.Knight.KnightHitEffect;


            base.OnEnter();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(SS2.Survivors.Knight.dodgeFOV, 60f, base.fixedAge / duration);
            base.characterMotor.moveDirection = base.inputBank.moveVector;

            fireAge += Time.fixedDeltaTime;
            base.characterBody.SetAimTimer(2f);
            attackSpeedStat = base.characterBody.attackSpeed;
            fireFrequency = baseFireFrequency * attackSpeedStat;

            if (fireAge >= 1f / fireFrequency && base.isAuthority)
            {
                fireAge = 0f;
                attack.ResetIgnoredHealthComponents();
                attack.isCrit = base.characterBody.RollCrit();
                attack.Fire();
            }
        }


        public override void OnExit()
        {
            if (characterMotor)
            {
                characterMotor.capsuleCollider.gameObject.layer = _origLayer;
                characterMotor.Motor.RebuildCollidableLayers();
            }

            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.OnExit();
        }

        public override void PlayAttackAnimation()
        {
            PlayCrossfade("FullBody, Override", "Utility", "Utility.playbackRate", duration * swingTimeCoefficient, 0.15f);   
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}