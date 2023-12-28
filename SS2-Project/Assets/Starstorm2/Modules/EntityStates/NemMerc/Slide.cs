using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using Moonstorm.Starstorm2.Components;

namespace EntityStates.NemMerc
{
    public class Slide : BaseSkillState
    {
        public static AnimationCurve forwardSpeedCurve;
        public static float forwardSpeedCurveCoefficient = 5f;
        public static float baseExtraForwardSpeed = 0f;
        public static float baseDuration = 1.5f;
        public static float minDuration = 0.5f;
        public static float maxTurnSpeed = 720f;
        public static float minTurnSpeed = 180f;
        public static float smoothTime = 0.1f;
        public static float airFrictionCoefficient = 0.4f;
        public static float movespeedScalingCoefficient = 0.67f;

        private Vector3 targetMoveVectorVelocity;
        private Vector3 forwardDirection;
        private GameObject slideEffectInstance;
        private bool m2Buffered;
        private NemMercTracker tracker;
        private float stopwatch;

        private EntityStateMachine body;

        private bool isClone;
        public override void OnEnter()
        {
            base.OnEnter();

            base.PlayAnimation("Body", "SlideStart");
            base.GetModelAnimator().SetFloat("inSlideState", 1);

            Util.PlaySound(Commando.SlideState.soundString, base.gameObject);//sound

            if (Commando.SlideState.slideEffectPrefab)
            {
                Transform parent = base.characterBody.transform;// base.FindModelChild("Base");
                this.slideEffectInstance = UnityEngine.Object.Instantiate<GameObject>(Commando.SlideState.slideEffectPrefab, parent);
            } //vfx

            //FOV

            this.body = EntityStateMachine.FindByCustomName(base.gameObject, "Body");
            this.tracker = base.GetComponent<NemMercTracker>();
            this.isClone = this.tracker is NemMercCloneTracker; // ???????????????????????

            if (base.inputBank && base.characterDirection)
            {
                base.characterDirection.forward = ((base.inputBank.moveVector == Vector3.zero) ? base.inputBank.aimDirection : base.inputBank.moveVector).normalized;
                this.forwardDirection = base.characterDirection.forward;
                base.characterDirection.moveVector = this.forwardDirection;
            }

            if (base.characterMotor)
            {
                base.characterMotor.velocity.y = Mathf.Max(0, base.characterMotor.velocity.y);
            }

            base.characterBody.SetSpreadBloom(0f, false);
        }

        private void GatherInputs()
        {
            GenericSkill secondary = base.skillLocator.secondary;
            //nametoken is stupid and lazy
            if (base.inputBank.skill2.justPressed && secondary.IsReady()
                && secondary.skillDef.skillNameToken == "SS2_NEMESIS_MERCENARY_SECONDARY_SLASH_NAME")
            {
                this.m2Buffered = true;

                //vfx and sound
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            //GatherInputs();

            if(base.isAuthority)
            {
                bool inMainState = !this.body || (this.body && this.body.IsInMainState());
                float time = base.isGrounded && inMainState ? Time.fixedDeltaTime : Time.fixedDeltaTime * airFrictionCoefficient;
                this.stopwatch += time;

                float t = Mathf.Clamp01(this.stopwatch / baseDuration);
                float turnSpeed = Mathf.Lerp(maxTurnSpeed, minTurnSpeed, t);
                float f = forwardSpeedCurve.Evaluate(t) * forwardSpeedCurveCoefficient;
                float forwardSpeed = f + baseExtraForwardSpeed;

                base.characterMotor.moveDirection = Vector3.zero;

                if (base.isGrounded)
                {
                    Vector3 input = base.inputBank.moveVector != Vector3.zero ? base.inputBank.moveVector : base.characterDirection.forward;
                    Vector3 target = Vector3.ProjectOnPlane(Vector3.SmoothDamp(this.forwardDirection, input, ref this.targetMoveVectorVelocity, smoothTime, turnSpeed), Vector3.up).normalized;
                    this.forwardDirection = target;                    
                    base.characterDirection.moveVector = target;
                }

                float msMultiplier = base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed;
                msMultiplier *= movespeedScalingCoefficient;
                if(inMainState && !this.isClone)
                    base.characterMotor.rootMotion += base.characterBody.moveSpeed * msMultiplier * forwardSpeed * this.forwardDirection * Time.fixedDeltaTime;

                
                if (!base.IsKeyDownAuthority() && base.fixedAge >= minDuration && inMainState)
                {
                    if (this.tracker)
                        this.outer.SetNextState(new TargetDash { m2Buffered = this.m2Buffered, target = this.tracker.GetTrackingTarget() });
                    else
                        this.outer.SetNextStateToMain();
                }
            }
        }

        public override void OnExit()
        {
            base.OnExit();

            base.PlayAnimation("Body", "SlideEnd");
            base.GetModelAnimator().SetFloat("inSlideState", 0);

            if (this.slideEffectInstance)
            {
                EntityState.Destroy(this.slideEffectInstance);
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
