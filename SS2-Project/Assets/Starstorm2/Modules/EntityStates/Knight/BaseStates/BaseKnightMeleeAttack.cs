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

namespace EntityStates.Knight
{
    public abstract class BaseKnightMeleeAttack : BaseSkillState, SteppedSkillDef.IStepSetter
    {
        // Unity Variables
        [SerializeField]
        public string hitboxGroupName = "SwordHitbox";

        [SerializeField]
        public DamageType damageType = DamageType.Generic;
        [SerializeField]
        public float damageCoefficient = 3.5f;
        [SerializeField]
        public float procCoefficient = 1f;
        [SerializeField]
        public float pushForce = 300f;
        [SerializeField]
        public Vector3 bonusForce = Vector3.zero;
        [SerializeField]
        public float baseDuration = 1f;

        [SerializeField]
        public float attackStartTimeFraction = 0f;
        [SerializeField]
        public float attackEndTimeFraction = 1f;

        [SerializeField]
        public float earlyExitPercentTime = 0.4f;

        [SerializeField]
        public float hitStopDuration = 0.012f;
        [SerializeField]
        public float attackRecoil = 0.75f;
        [SerializeField]
        public float hitHopVelocity = 4f;

        [SerializeField]
        public string swingSoundString = "";
        [SerializeField]
        public string hitSoundString = "";
        [SerializeField]
        public string muzzleString = "SwingCenter";
        [SerializeField]
        public string playbackRateParam = "Slash.playbackRate";
        [SerializeField]
        public GameObject swingEffectPrefab;
        [SerializeField]
        public GameObject hitEffectPrefab;
        [SerializeField]
        public NetworkSoundEventDef impactSound;

        [SerializeField]
        public InterruptPriority earlyExitPriority = InterruptPriority.Any;
        [SerializeField]
        public InterruptPriority basePriority = InterruptPriority.Skill;

        public bool addModdedDamageType = false;
        public DamageAPI.ModdedDamageType moddedDamageType;

        public int swingIndex;

        protected OverlapAttack attack;
        protected float duration;
        protected bool hasFired;
        protected Animator animator;
        protected bool inHitPause;
        protected float stopwatch;

        private float hitPauseTimer;
        private bool hasHopped;
        private HitStopCachedState hitStopCachedState;
        private Vector3 storedVelocity;

        protected virtual void ModifyMelee() { }

        public override void OnEnter()
        {
            base.OnEnter();

            ModifyMelee();

            duration = baseDuration / attackSpeedStat;
            animator = GetModelAnimator();
            StartAimMode(2f + duration, false);

            PlayAttackAnimation();

            if (string.IsNullOrEmpty(hitboxGroupName))
                return;

            attack = new OverlapAttack();
            attack.damageType = damageType;
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
            if (impactSound != null)
            {
                attack.impactSound = impactSound.index;
            }
            if (addModdedDamageType)
            {
                attack.AddModdedDamageType(moddedDamageType);
            }
        }

        public virtual void PlayAttackAnimation()
        {
        }

        public override void OnExit()
        {
            if (inHitPause)
            {
                RemoveHitstop();
            }
            base.OnExit();
        }

        protected virtual void PlaySwingEffect()
        {
            EffectManager.SimpleMuzzleFlash(swingEffectPrefab, gameObject, muzzleString, false);
        }

        protected virtual void OnHitEnemyAuthority()
        {
            Util.PlaySound(hitSoundString, gameObject);

            if (!hasHopped)
            {
                if (characterMotor && !characterMotor.isGrounded && hitHopVelocity > 0f)
                {
                    SmallHop(characterMotor, hitHopVelocity);
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

        private void FireAttack()
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
                AddRecoil(-1f * attackRecoil, -2f * attackRecoil, -0.5f * attackRecoil, 0.5f * attackRecoil);
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
            }
            else
            {
                if (characterMotor) characterMotor.velocity = Vector3.zero;
                if (animator) animator.SetFloat(playbackRateParam, 0f);
            }
            bool fireStarted = stopwatch >= duration * attackStartTimeFraction;
            bool fireEnded = stopwatch >= duration * attackEndTimeFraction;

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
                SetNextState();
                return;
            }
        }

        protected virtual void SetNextState()
        {
            outer.SetNextStateToMain();
        }

        private void RemoveHitstop()
        {
            ConsumeHitStopCachedState(hitStopCachedState, characterMotor, animator);
            inHitPause = false;
            characterMotor.velocity = storedVelocity;
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {

            if (stopwatch >= duration * earlyExitPercentTime)
            {
                return earlyExitPriority;
            }
            return basePriority;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(swingIndex);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            swingIndex = reader.ReadInt32();
        }

        public void SetStep(int i)
        {
            swingIndex = i;
        }
    }
}
