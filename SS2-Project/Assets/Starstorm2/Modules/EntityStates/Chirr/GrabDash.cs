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
        public static float dashSpeed = 9f;
        public static float minGrabRadius = 4f;
        public static float maxGrabRadius = 8f;
        public static float grabSearchFrequency = 10f;
        public static float maxTurnAnglePerSecond = 30f;

        private float stopwatch;
        private float duration; 
        private Vector3 currentAimVector;
        private AnimationCurve speedCurve;

        private float grabSearchTimer;
        private GameObject debugSphere;

        private GrabController grabController;
        public override void OnEnter()
        {
            base.OnEnter();
            this.speedCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
            this.duration = baseDuration; // ?

            this.debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            this.debugSphere.transform.position = base.characterBody.corePosition;
            this.debugSphere.transform.localScale = Vector3.one * minGrabRadius;
            Destroy(this.debugSphere.GetComponent<Collider>());

            this.currentAimVector = base.inputBank.aimDirection;
            this.grabController = base.GetComponent<GrabController>();
            //Util.PlaySound();
            //base.PlayAnimation();
            //GRAB TRANSFORM CHILDLOACTOR
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.stopwatch += Time.fixedDeltaTime;

            Vector3 aimInput = base.inputBank.aimDirection;
            this.currentAimVector = Vector3.RotateTowards(this.currentAimVector, aimInput, Mathf.Deg2Rad * maxTurnAnglePerSecond * Time.deltaTime, 0);

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

                    this.debugSphere.transform.position = base.characterBody.corePosition;
                    this.debugSphere.transform.localScale = Vector3.one * grabRadius;
                }
            }

            if(base.fixedAge >= this.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
            
            
        }
        public override void OnExit()
        {
            base.OnExit();
            if (this.debugSphere) Destroy(this.debugSphere);
        }

        public void AttemptGrab(float grabRadius)
		{
            // could be better as a bullseyesearch? doesnt really matter tho
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
					if (hurtBox.healthComponent && hurtBox.healthComponent.body && hurtBox.healthComponent.gameObject != base.gameObject)
					{
						if (!hurtBox.healthComponent.body.isChampion || (hurtBox.healthComponent.gameObject.name.Contains("Brother") && hurtBox.healthComponent.gameObject.name.Contains("Body")))
						{
                            //Vector3 between = hurtBox.healthComponent.transform.position - base.transform.position;
                            //Vector3 v = between / 4f;
                            //v.y = Math.Max(v.y, between.y);
                            //base.characterMotor.AddDisplacement(v);
                            // ^^^ SNAP TO VICTIM

                            this.OnGrabBodyAuthority(hurtBox.healthComponent.body);
                            

							
						}
					}
				}
			}
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
