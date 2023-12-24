using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using Moonstorm.Starstorm2.Components;
namespace EntityStates.Chirr
{
    public class GrabDash : BaseSkillState
    {
        public static float baseDuration = 0.75f;
        public static float dashSpeed = 7.5f;
        public static float minGrabRadius = 4f;
        public static float maxGrabRadius = 8f;
        public static float grabSearchFrequency = 20f;
        public static float maxTurnAnglePerSecond = 30f;

        private float stopwatch;
        private float duration; 
        private Vector3 currentAimVector;
        private AnimationCurve speedCurve;

        private float grabSearchTimer;

        private GrabController grabController;
        public override void OnEnter()
        {
            base.OnEnter();
            this.speedCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.2f);
            this.duration = baseDuration; // ?

            this.currentAimVector = base.inputBank.aimDirection;

            base.characterDirection.forward = this.currentAimVector;
            this.grabController = base.GetComponent<GrabController>();
            Util.PlaySound("ChirrDashStart", base.gameObject);
            base.PlayAnimation("FullBody, Override", "GrabDash", "Utility.playbackRate", this.duration);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            Vector3 aimInput = base.inputBank.aimDirection;
            this.currentAimVector = Vector3.RotateTowards(this.currentAimVector, aimInput, Mathf.Deg2Rad * maxTurnAnglePerSecond * Time.deltaTime, 0);

            base.characterDirection.forward = this.currentAimVector;

            float dashSpeed = this.speedCurve.Evaluate(this.stopwatch / duration) * GrabDash.dashSpeed;
            base.characterMotor.rootMotion += this.currentAimVector * dashSpeed * this.moveSpeedStat * Time.fixedDeltaTime;
            base.characterMotor.velocity.y = 0f;

            if(base.isAuthority)
            {
                this.grabSearchTimer -= Time.fixedDeltaTime;
                if (grabSearchTimer <= 0f)
                {
                    grabSearchTimer += 1 / grabSearchFrequency;
                    
                    float grabRadius = Mathf.Lerp(minGrabRadius, maxGrabRadius, this.stopwatch / duration);
                    this.AttemptGrab(grabRadius);
                }
            }

            if(base.fixedAge >= this.duration)
            {
                base.characterMotor.velocity = this.currentAimVector * dashSpeed * this.moveSpeedStat;
                this.outer.SetNextStateToMain();
                return;
            }
            
            
        }
        public override void OnExit()
        {
            base.OnExit();
        }

        public void AttemptGrab(float grabRadius)
		{
            SphereSearch sphereSearch = new SphereSearch();
            sphereSearch.radius = grabRadius;
            sphereSearch.origin = base.characterBody.corePosition;
            sphereSearch.queryTriggerInteraction = QueryTriggerInteraction.Ignore;
            sphereSearch.mask = LayerIndex.entityPrecise.mask;
            sphereSearch.RefreshCandidates();
            sphereSearch.FilterCandidatesByDistinctHurtBoxEntities();
            HurtBox[] hurtBoxes = sphereSearch.GetHurtBoxes();
			foreach (HurtBox hurtBox in hurtBoxes)
			{
				if (hurtBox)
				{
                    HealthComponent healthComponent = hurtBox.healthComponent;
					if (healthComponent && healthComponent.gameObject != base.gameObject)
					{
						if (!healthComponent.body.isChampion || (healthComponent.gameObject.name.Contains("Brother") && healthComponent.gameObject.name.Contains("Body")) || healthComponent.body.GetComponent<NemesisResistances>())
						{
                            //Vector3 between = hurtBox.healthComponent.transform.position - base.transform.position;
                            //Vector3 v = between / 4f;
                            //v.y = Math.Max(v.y, between.y);
                            //base.characterMotor.AddDisplacement(v);
                            // ^^^ SNAP TO VICTIM
                            if(BodyIsGrabbable(healthComponent.gameObject)) 
                                this.OnGrabBodyAuthority(hurtBox.healthComponent.body);							
						}
					}
				}
			}
		}

        // cant grab something that is grabbing us
        // cant grab something that is being grabbed (nvm)
        private bool BodyIsGrabbable(GameObject candidate)
        {
            if(GrabController.victimsToGrabControllers.TryGetValue(base.gameObject, out GrabController grabController))
            {
                if (grabController && (grabController.gameObject != candidate || grabController.victimBodyObject != null))
                    return false;
            }
            return true;
        }

        public void OnGrabBodyAuthority(CharacterBody body)
        {
            if(this.grabController)
                this.grabController.AttemptGrab(body.gameObject);

            this.outer.SetNextStateToMain();
            EntityStateMachine wingsStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Wings");
            if (wingsStateMachine) wingsStateMachine.SetNextState(new Wings.BoostUp());

            EntityStateMachine clawsStateMachine = EntityStateMachine.FindByCustomName(base.gameObject, "Claws");
            if (clawsStateMachine) clawsStateMachine.SetNextState(new Claws.Grabbing());
        }

		public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Pain;
        }
    }
}
