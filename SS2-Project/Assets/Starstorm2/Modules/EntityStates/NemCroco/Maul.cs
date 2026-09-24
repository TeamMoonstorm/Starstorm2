using EntityStates;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.Skills;
using SS2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering.PostProcessing;

namespace EntityStates.NemCroco
{
    public class Maul : BaseSkillState, RoR2.Skills.SteppedSkillDef.IStepSetter
    {
        private static string hitboxGroupName = "Swipe";
        private static float damageCoefficient = 2.4f;
        private static float procCoefficient = 1f;
        private static float pushForce = 300f;
        private static Vector3 bonusForce = Vector3.zero;

        private static float baseDuration = 1.33f;
        private static float animDuration = 1.33f;

        private static float attackStartTime = 0.15f;
        private static float attackEndTime = 0.33f;
        private static float earlyExitTime = 0.5f;

        private static float hitStopDuration = 0.012f;
        private static float recoil = 0.75f;
        private static float bloom = 1f;
        private static float hitHopVelocity = 4f;

        private static string enterSoundString = "Play_acrid_m2_bite_shoot";
        private static string swingSoundString = "";
        private static string hitSoundString = "";
        private static string playbackRateParam = "Swipe.playbackRate";

        private static float forwardSpeedCoefficient = 3f;
        public static AnimationCurve forwardSpeedCurve;
        public static GameObject swingEffectPrefab;
        public static GameObject hitEffectPrefab;
        public static NetworkSoundEventDef impactSound;

        protected OverlapAttack attack;
        protected float duration;
        protected bool hasFired;
        protected Animator animator;
        protected bool inHitPause;
        protected float stopwatch;

        private float hitPauseTimer;
        private bool hasHopped;
        private HitStopCachedState hitStopCachedState;
        protected Vector3 storedVelocity;

        private GameObject swingEffectInstance;
        private EffectManagerHelper swingEffectInstanceHelper;
        private ScaleParticleSystemDuration swingEffectParticleScaler;

        private string muzzleString;
        private int step;
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            if (stopwatch >= duration * earlyExitTime)
            {
                return InterruptPriority.Skill;
            }
            return InterruptPriority.PrioritySkill;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            writer.Write((byte)step);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            step = reader.ReadByte();
        }
        public void SetStep(int i)
        {
            step = i;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / attackSpeedStat;
            animator = GetModelAnimator();
            StartAimMode(2f + duration, false);

            Util.PlaySound(enterSoundString, gameObject);

            if (step % 2 == 0)
            {
                muzzleString = "SwipeLeft";
                PlayCrossfade("Gesture, Additive", "SwipeLeft", "Swipe.playbackRate", animDuration, 0.05f);
                PlayCrossfade("Gesture, Override", "SwipeLeft", "Swipe.playbackRate", animDuration, 0.05f);
            }
            else
            {
                muzzleString = "SwipeRight";
                PlayCrossfade("Gesture, Additive", "SwipeRight", "Swipe.playbackRate", animDuration, 0.05f);
                PlayCrossfade("Gesture, Override", "SwipeRight", "Swipe.playbackRate", animDuration, 0.05f);
            }
            

            if (string.IsNullOrEmpty(hitboxGroupName))
                return;

            attack = new OverlapAttack();
            attack.damageType = DamageTypeCombo.GenericPrimary;
            attack.attacker = gameObject;
            attack.inflictor = gameObject;
            attack.teamIndex = GetTeam();
            attack.damage = damageCoefficient * damageStat;
            attack.procCoefficient = procCoefficient;
            attack.hitEffectPrefab = hitEffectPrefab;
            attack.forceVector = bonusForce;
            attack.pushAwayForce = pushForce;
            attack.hitBoxGroup = FindHitBoxGroup(hitboxGroupName);
            attack.isCrit = RollCrit();
            attack.maximumOverlapTargets = 1000;


            attack.AddModdedDamageType(SS2.Survivors.NemCroco.NemesisPoisonOnHit);


            if (impactSound != null)
            {
                attack.impactSound = impactSound.index;
            }
        }

        public override void OnExit()
        {
            if (inHitPause)
            {
                RemoveHitstop();
            }
            base.OnExit();
        }

        protected void PlaySwingEffect()
        {
            Transform transform = base.FindModelChild(muzzleString);
            if (transform)
            {
                if (!EffectManager.ShouldUsePooledEffect(swingEffectPrefab))
                {
                    this.swingEffectInstance = UnityEngine.Object.Instantiate<GameObject>(swingEffectPrefab, transform);
                }
                else
                {
                    this.swingEffectInstanceHelper = EffectManager.GetAndActivatePooledEffect(swingEffectPrefab, transform, true);
                    this.swingEffectInstance = this.swingEffectInstanceHelper.gameObject;
                }

                swingEffectParticleScaler = this.swingEffectInstance.GetComponent<ScaleParticleSystemDuration>();
                if (swingEffectParticleScaler)
                {
                    swingEffectParticleScaler.newDuration = swingEffectParticleScaler.initialDuration;
                }
            }
        }

        protected void OnHitEnemyAuthority()
        {
            Util.PlaySound(hitSoundString, gameObject);
            characterBody.AddSpreadBloom(bloom);

            if (!hasHopped)
            {
                if (characterMotor && !characterMotor.isGrounded && hitHopVelocity > 0f)
                {
                    SmallHop(characterMotor, hitHopVelocity / (attackSpeedStat * attackSpeedStat));
                }

                hasHopped = true;
            }

            ApplyHitstop();
        }

        protected void ApplyHitstop()
        {
            if (!inHitPause && hitStopDuration > 0f)
            {
                storedVelocity = characterMotor.velocity;
                hitStopCachedState = CreateHitStopCachedState(characterMotor, animator, playbackRateParam);
                hitPauseTimer = hitStopDuration / attackSpeedStat;
                inHitPause = true;
            }
        }

        protected void FireAttack()
        {
            if (isAuthority && attack != null)
            {
                if (attack.Fire())
                {
                    OnHitEnemyAuthority();
                }
            }
        }

        private void EnterAttack()
        {
            hasFired = true;
            Util.PlayAttackSpeedSound(swingSoundString, gameObject, attackSpeedStat);

            PlaySwingEffect();

            if (isAuthority)
            {
                AddRecoil(-1f * recoil, -2f * recoil, -0.5f * recoil, 0.5f * recoil);
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            hitPauseTimer -= Time.fixedDeltaTime;

            if (hitPauseTimer <= 0f && inHitPause)
            {
                RemoveHitstop();
            }

            if (!inHitPause)
            {
                stopwatch += Time.fixedDeltaTime;
                ApplyMovement();
            }
            else
            {
                if (characterMotor) characterMotor.velocity = Vector3.zero;
                if (animator) animator.SetFloat(playbackRateParam, 0f);
            }
            bool fireStarted = stopwatch >= duration * attackStartTime;
            bool fireEnded = stopwatch >= duration * attackEndTime;

            //to guarantee attack comes out if at high attack speed the stopwatch skips past the firing duration between frames
            if (fireStarted && !fireEnded || fireStarted && fireEnded && !hasFired)
            {
                if (!hasFired)
                {
                    EnterAttack();
                }
                FireAttack();
            }

            if (stopwatch >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
                return;
            }
        }

        public void ApplyMovement()
        {
            float t = Mathf.Clamp01(fixedAge / duration);
            float forwardSpeed = moveSpeedStat * forwardSpeedCoefficient * forwardSpeedCurve.Evaluate(t);
            Vector3 targetVector = inputBank.aimDirection;

            characterDirection.targetVector = targetVector;
            characterMotor.AddDisplacement(characterDirection.forward * forwardSpeed * Time.fixedDeltaTime);
        }

        private void RemoveHitstop()
        {
            ConsumeHitStopCachedState(hitStopCachedState, characterMotor, animator);
            inHitPause = false;
            characterMotor.velocity = storedVelocity;

            if (this.swingEffectInstance)
            {
                if (swingEffectParticleScaler)
                {
                    swingEffectParticleScaler.newDuration = swingEffectParticleScaler.initialDuration;
                }
            }
        }

        
    }
}
