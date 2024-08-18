using EntityStates;
using EntityStates.Knight;
using MSU;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;

namespace Assets.Starstorm2.Modules.EntityStates.Knight.BuffedSkills
{
    public class TornadoSpin : BaseKnightMeleeAttack
    {
        public static float swingTimeCoefficient = 1f;
        [FormatToken("SS2_KNIGHT_SPECIAL_SPIN_DESC", FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float TokenModifier_dmgCoefficient => new SpinUtility().damageCoefficient;

        public static GameObject beamProjectile;
        public static SkillDef originalSkillRef;

        private int _origLayer;

        public float hopVelocity = new SpinUtility().hopVelocity;
        public float airControl = new SpinUtility().airControl;
        public float upwardVelocity = new SpinUtility().upwardVelocity;
        public float forwardVelocity = new SpinUtility().forwardVelocity + 1f;
        public float minimumY = new SpinUtility().minimumY;
        public float aimVelocity = new SpinUtility().aimVelocity;

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
                //base.characterMotor.Motor.ForceUnground();
                base.characterMotor.velocity = val + val2 + val4;
            }

            if (!isGrounded)
            {
                SmallHop(characterMotor, hopVelocity);
            }


            hitboxGroupName = "BigHitbox";

            damageType = DamageType.Stun1s;
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

            hitStopDuration = 0.014f;
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