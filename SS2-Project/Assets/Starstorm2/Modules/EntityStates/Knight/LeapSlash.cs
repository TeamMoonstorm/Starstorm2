using Moonstorm.Starstorm2;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace EntityStates.Knight
{
    public class LeapSlash : BaseState
    {
        public static float baseDamageCoefficient;
        public static float baseDuration;
        private float duration;

        public static float airControl;
        private float previousAirControl;
        public static float upwardVelocity;
        public static float forwardVelocity;
        public static float knockbackForce;
        public static float minimumY;
        public static float aimVelocity;

        public static float baseDurationBetweenSwings;
        private float durationBetweenSwings;
        public static float baseSwingCount;
        private float totalSwings = 0;
        public static float radius;
        private float swingCount;
        public float stopwatchBetweenSwings;

        public static GameObject beamProjectile;

        private Animator animator;
        public static float hitPauseDuration;
        public static float shorthopVelocityFromHit;
        protected float hitPauseTimer;
        protected Vector3 storedHitPauseVelocity;

        public static GameObject swingEffectPrefab;
        public static string swingSoundString;
        public static string swingEffectMuzzleString;
        private GameObject swingEffectInstance;

        private BlastAttack blastAttack;
        private bool swingSide;

        protected bool authorityInHitPause
        {
            get
            {
                return hitPauseTimer > 0f;
            }
        }

        public override void OnEnter()
        {
            base.OnEnter();

            animator = GetModelAnimator();
            duration = baseDuration;
            durationBetweenSwings = baseDurationBetweenSwings / attackSpeedStat;
            swingCount = baseSwingCount * attackSpeedStat;

            characterBody.bodyFlags |= CharacterBody.BodyFlags.IgnoreFallDamage;
            previousAirControl = characterMotor.airControl;
            characterMotor.airControl = airControl;
            Vector3 direction = GetAimRay().direction;

            blastAttack = new BlastAttack()
            {
                attacker = gameObject,
                attackerFiltering = AttackerFiltering.NeverHitSelf,
                baseDamage = baseDamageCoefficient * damageStat,
                baseForce = -500f,
                bonusForce = Vector3.zero,
                crit = RollCrit(),
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BlastAttack.FalloffModel.None,
                inflictor = gameObject,
                losType = BlastAttack.LoSType.None,
                position = characterBody.corePosition,
                procChainMask = default,
                procCoefficient = 1f,
                radius = radius,
                teamIndex = GetTeam()
            };

            if (isAuthority)
            {
                Swing();

                characterBody.isSprinting = true;
                direction.y = Mathf.Max(direction.y, minimumY);
                Vector3 a = direction.normalized * aimVelocity * moveSpeedStat;
                Vector3 b = Vector3.up * upwardVelocity;
                Vector3 b2 = new Vector3(direction.x, 0f, direction.z).normalized * forwardVelocity;
                characterMotor.Motor.ForceUnground();
                characterMotor.velocity = a + b + b2;
            } 
        }

        protected virtual void BeginMeleeAttackEffect()
        {
            Util.PlaySound(swingSoundString, gameObject);
            if (swingEffectPrefab)
            {
                Transform muzzle = FindModelChild(swingEffectMuzzleString);
                if (muzzle)
                {
                    swingEffectInstance = Object.Instantiate(swingEffectPrefab, muzzle);
                    ScaleParticleSystemDuration spsd = swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                    if (spsd)
                    {
                        spsd.newDuration = spsd.initialDuration;
                    }
                }
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            stopwatchBetweenSwings += Time.fixedDeltaTime;

            if (stopwatchBetweenSwings >= durationBetweenSwings && totalSwings < swingCount)
            {
                Debug.Log("Swinging");
                stopwatchBetweenSwings -= durationBetweenSwings;
                Swing();
                totalSwings++; 
            }

            if (isAuthority)
                AuthorityFixedUpdate();
            
            if (totalSwings == swingCount || fixedAge >= duration)
            {
                outer.SetNextStateToMain();
            }
        }

        private void AuthorityFixedUpdate()
        {
            if (authorityInHitPause)
            {
                hitPauseTimer -= Time.fixedDeltaTime;
                if (characterMotor)
                    characterMotor.velocity = Vector3.zero;
                fixedAge -= Time.fixedDeltaTime;
                if (!authorityInHitPause)
                    AuthorityExitHitPause();
            }
        }

        protected virtual void AuthorityExitHitPause()
        {
            hitPauseTimer = 0f;
            storedHitPauseVelocity.y = Mathf.Max(storedHitPauseVelocity.y, shorthopVelocityFromHit / Mathf.Sqrt(attackSpeedStat));
            if (characterMotor)
            {
                characterMotor.velocity = storedHitPauseVelocity;
            }
            storedHitPauseVelocity = Vector3.zero;
            if (animator)
                animator.speed = 1f;
        }

        private void AuthorityTriggerHitPause()
        {
            if (characterMotor)
            {
                storedHitPauseVelocity += characterMotor.velocity;
                characterMotor.velocity = Vector3.zero;
            }
            if (animator)
                animator.speed = 0f;
            hitPauseTimer = hitPauseDuration / attackSpeedStat;
        }

        private void Swing()
        {
            if (isAuthority)
            {
                blastAttack.position = FindModelChild(swingEffectMuzzleString).position;
                int hitcount = blastAttack.Fire().hitCount;
                if (hitcount > 0)
                {
                    Util.PlaySound(Merc.GroundLight.hitSoundString, gameObject);
                    AuthorityTriggerHitPause();
                }

                BeginMeleeAttackEffect();

                string animationStateName = swingSide ? "SwingSword1" : "SwingSword2";
                PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", durationBetweenSwings, 0.02f);
                swingSide = !swingSide;

                if (characterBody.HasBuff(SS2Content.Buffs.bdKnightBuff) && isAuthority)
                    ProjectileManager.instance.FireProjectile(
                        beamProjectile,
                        GetAimRay().origin,
                        Util.QuaternionSafeLookRotation(GetAimRay().direction),
                        gameObject,
                        damageStat * baseDamageCoefficient,
                        0f,
                        RollCrit(),
                        DamageColorIndex.Default,
                        null,
                        80f);
            }

            
        }

        public override void OnExit()
        {
            characterMotor.airControl = previousAirControl;
            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            if (authorityInHitPause)
                AuthorityExitHitPause();
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
