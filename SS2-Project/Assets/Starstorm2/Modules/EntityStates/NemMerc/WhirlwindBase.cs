using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using Moonstorm.Starstorm2.DamageTypes;

namespace EntityStates.NemMerc
{
    public class WhirlwindBase : BaseSkillState
    {
        [SerializeField]
        public float walkSpeedCoefficient = 0.5f;
        [SerializeField]
        public float damageCoefficient = 3.5f;
        [SerializeField]
        public float smallHopVelocity = 8f;
        [SerializeField]
        public float baseDuration = 0.5f;
        [SerializeField]
        public float numAttacks = 2;
        [SerializeField]
        public float hitPauseDuration = 0.05f;
        [SerializeField]
        public float entryVelocityDamp = 0.5f;
        [SerializeField]
        public float attackSoundPitch = 1f;

        [SerializeField]
        public string hitboxGroupName;
        [SerializeField]
        public string muzzleName;
        [SerializeField]
        public string attackSoundString = "NemmandoFireBeam2";
        [SerializeField]
        public GameObject hitEffectPrefab;
        [SerializeField]
        public GameObject swingEffectPrefab;
        [SerializeField]
        public  InterruptPriority interruptPriority = InterruptPriority.PrioritySkill;

        private Animator animator;
        private float duration;
        private float timeBetweenAttacks;
        private float attackStopwatch;
        private OverlapAttack attack;
        private float timesAttacked;

        [NonSerialized]
        public bool hitEnemy;

        private bool isInHitPause;
        private float hitPauseTimer;
        private HitStopCachedState hitStopCachedState;

        public override void OnEnter()
        {
            base.OnEnter();

            base.StartAimMode();
            this.animator = base.GetModelAnimator();
            //anim

            this.duration = this.baseDuration / this.attackSpeedStat;
            this.timeBetweenAttacks = this.duration / numAttacks;

            this.attack = base.InitMeleeOverlap(this.damageCoefficient, this.hitEffectPrefab, base.GetModelTransform(), this.hitboxGroupName);
            this.attack.AddModdedDamageType(RedirectHologram.damageType);

            base.characterMotor.velocity = Vector3.zero;
            base.characterMotor.walkSpeedPenaltyCoefficient = this.walkSpeedCoefficient;

            base.PlayAnimation("FullBody, Override", "SecondarySlash", "Secondary.playbackRate", this.duration * 0.825f);
            base.PlayAnimation("Gesture, Override", "SecondarySlash", "Secondary.playbackRate", this.duration * 0.825f);
            //"momentum"
            //i dont like it          
            //if(this.hitEnemy) 
            //{
            //    base.characterMotor.velocity = Vector3.zero;
            //    base.characterMotor.walkSpeedPenaltyCoefficient = this.walkSpeedCoefficient;
            //}
            //else
            //{
            //    base.characterMotor.velocity *= this.walkSpeedCoefficient;
            //}

        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if(!this.isInHitPause)
            {
                this.attackStopwatch -= Time.fixedDeltaTime;
                base.characterMotor.velocity.y = Mathf.Max(base.characterMotor.velocity.y, 0);
                if (this.attackStopwatch <= 0f && this.timesAttacked < this.numAttacks)
                {
                    //vfx
                    //sound  
                    this.FireAttack();
                    if (this.timesAttacked == this.numAttacks && !base.isGrounded)
                    {
                        base.SmallHop(base.characterMotor, this.smallHopVelocity);
                        base.characterMotor.walkSpeedPenaltyCoefficient = 1;
                    }
                }
            }
            else
            {
                base.characterMotor.velocity = Vector3.zero;
                this.hitPauseTimer -= Time.fixedDeltaTime;
                this.animator.SetFloat("Secondary.playbackRate", 0f);
            }          
            if (base.isAuthority)
            {
                // if ANIM PARAM
                if (this.attack.Fire())
                {
                    this.OnHitEnemyAuthority();
                    //if(!this.hitEnemy)
                    //{
                    //    this.hitEnemy = true;
                    //    base.characterMotor.velocity = Vector3.zero;
                    //    base.characterMotor.walkSpeedPenaltyCoefficient = this.walkSpeedCoefficient;
                    //}
                }

                if (this.hitPauseTimer <= 0f && this.isInHitPause)
                {
                    base.ConsumeHitStopCachedState(this.hitStopCachedState, base.characterMotor, this.animator);
                    this.isInHitPause = false;
                }

                if (base.fixedAge >= this.duration)
                {
                    this.outer.SetNextStateToMain();
                    return;
                }
            }
            
        }

        private void FireAttack()
        {
            Util.PlayAttackSpeedSound(this.attackSoundString, base.gameObject, this.attackSoundPitch);
            EffectManager.SimpleMuzzleFlash(this.swingEffectPrefab, base.gameObject, this.muzzleName, false);
            this.attackStopwatch += this.timeBetweenAttacks;
            this.attack.ResetIgnoredHealthComponents();
            this.timesAttacked++;
        }

        public override void OnExit()
        {
            base.OnExit();

            base.characterMotor.walkSpeedPenaltyCoefficient = 1f;
            //base.PlayAnimation("Gesture, Override", "KnifeToShotgun", "KnifeToShotgun.playbackRate", 1f);

            while (this.timesAttacked < this.numAttacks)
            {
                this.FireAttack();
                this.attack.Fire();
            }

        }

        private void OnHitEnemyAuthority()
        {             
            if (!this.isInHitPause)
            {
                this.hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, this.animator, "Secondary.playbackRate");
                this.hitPauseTimer = this.hitPauseDuration / this.attackSpeedStat;
                this.isInHitPause = true;
            }
        }


        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return this.timesAttacked < this.numAttacks ? InterruptPriority.PrioritySkill : InterruptPriority.Skill;/////////////////////////////////////////////////
        }
    }
}
