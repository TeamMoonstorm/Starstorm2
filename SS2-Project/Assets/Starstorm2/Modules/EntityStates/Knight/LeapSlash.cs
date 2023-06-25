using RoR2;
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

        private BlastAttack blastAttack;
        private bool swingSide;

        public override void OnEnter()
        {
            base.OnEnter();

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

            
            if (fixedAge >= duration || (fixedAge >= duration * 0.1f && isGrounded))
            {
                outer.SetNextStateToMain();
            }
        }

        private void Swing()
        {
            if (isAuthority)
            {
                blastAttack.position = characterBody.corePosition;
                int hitcount = blastAttack.Fire().hitCount;
                if (hitcount > 0) Util.PlaySound(Merc.GroundLight.hitSoundString, gameObject);

                string animationStateName = swingSide ? "SwingSword1" : "SwingSword2";
                PlayCrossfade("Gesture, Override", animationStateName, "Primary.playbackRate", durationBetweenSwings * 0.5f, 0.02f);
                swingSide = !swingSide;
            }
        }

        public override void OnExit()
        {
            characterMotor.airControl = previousAirControl;
            characterBody.bodyFlags &= ~CharacterBody.BodyFlags.IgnoreFallDamage;
            base.OnExit();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
