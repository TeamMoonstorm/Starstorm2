using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using Moonstorm.Starstorm2.Components;
using EntityStates.Huntress;
using Moonstorm.Starstorm2;
using UnityEngine.Networking;
namespace EntityStates.Chirr
{
    public class AimDrop : BaseSkillState
    {
		public static float splashRadius = 10f;
		public static float damageCoefficient = 15f;
		public static float dropMaxHorizontalVelocity = 20f;
		public static float minimumThrowDistance = 20f;
		public static float dropConeAngle = 55f;
		public static float maxRayDistance = 50f;
		public static float extraGravity = -15f;

		public static float selfAwayForce = 10f;
		public static float selfUpForce = 10f;

		private GameObject areaIndicatorInstance;

		private GrabController grabController;
		private ChirrGrabBehavior chirrGrabBehavior;

		private Vector3 desiredTrajectory;

		public override void OnEnter()
        {
            base.OnEnter();

			base.StartAimMode();
			this.grabController = base.GetComponent<GrabController>();
			this.chirrGrabBehavior = base.GetComponent<ChirrGrabBehavior>();
			if (ArrowRain.areaIndicatorPrefab)
			{
				this.areaIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(ArrowRain.areaIndicatorPrefab);
				this.areaIndicatorInstance.transform.localScale = new Vector3(ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius);
			}
		}

        public override void FixedUpdate()
        {
            base.FixedUpdate();
			
			if(base.isAuthority && !base.IsKeyDownAuthority())
            {
				this.outer.SetNextStateToMain();
            }
        }
        public override void Update()
		{
			base.Update();
			this.CalculateTrajectory();
		}

		private void CalculateTrajectory()
        {
			// NEED TO ACCOUNT FOR VICTIM'S HEIGHT/FOOT POSITION or something like that. UNDERSHOOTS OTHERWISE
			// looking straight down, max angle can only be dropConeAngle degrees "upwards"
			Vector3 down = Vector3.down;
			Vector3 aimOrigin = base.inputBank.aimOrigin;
			Vector3 aimDirection = Vector3.RotateTowards(down, base.inputBank.aimDirection, Mathf.Deg2Rad * dropConeAngle, 0f);
			RaycastHit raycastHit;
			Vector3 hitPoint = aimDirection * maxRayDistance + aimOrigin;
			
			if(Physics.Raycast(aimOrigin, aimDirection, out raycastHit, maxRayDistance, LayerIndex.world.mask))
            {
				hitPoint = raycastHit.point;
			}
			if (this.areaIndicatorInstance)
			{
				this.areaIndicatorInstance.transform.position = hitPoint;
				this.areaIndicatorInstance.transform.up = hitPoint;
			}

			//calculate trajectory with no upwards velocity
			float flightDuration = Trajectory.CalculateFlightDuration(aimOrigin.y, hitPoint.y, 0f, Physics.gravity.y + extraGravity);

			Vector3 hBetween = hitPoint - aimOrigin;
			hBetween.y = 0;
			float hDistance = hBetween.magnitude;
			float hSpeed = hDistance / flightDuration;

			this.desiredTrajectory = hSpeed * hBetween.normalized;
		}


		public override void OnExit()
		{
			base.OnExit();

            if (base.isAuthority)
            {
                Vector3 awayForce = -1f * base.inputBank.aimDirection * selfAwayForce;
                awayForce += Vector3.up * selfUpForce;
                if (base.characterMotor && !base.isGrounded)
                {
					// the skilldef has cancel sprint=false, this apparently is stopping sprint anyways
					float upVelocity = Mathf.Max(base.characterMotor.velocity.y, awayForce.y);
					base.characterMotor.velocity = new Vector3(characterMotor.velocity.x + awayForce.x, upVelocity, characterMotor.velocity.z + awayForce.z);
					if (EntityStateMachine.FindByCustomName(base.gameObject, "Wings")?.state.GetType() != typeof(Idle)) // jank to keep sprinting while hovering
                    {
                        base.characterBody.isSprinting = true;
                    }
                }
            }

            if (!this.outer.destroying && this.grabController && this.grabController.IsGrabbing())
			{
                Util.PlaySound("ChirrGrabThrow", base.gameObject);
				base.PlayAnimation("FullBody, Override", "GrabThrow");
				base.StartAimMode();
				bool isFriend = this.grabController.victimInfo.body.teamComponent.teamIndex == this.teamComponent.teamIndex;


				if (NetworkServer.active && this.chirrGrabBehavior)
                {
					GameObject victim = this.grabController.victimBodyObject;
					this.grabController.AttemptGrab(null);
					this.chirrGrabBehavior.ThrowVictim(victim, this.desiredTrajectory, extraGravity, isFriend);
                }
				
			}
			else
            {
				SS2Log.Warning("Chirr.AimDrop: didn't see victim! This is fine if you are a client. IsGrabbing = " + this.grabController.IsGrabbing());
            }
			if (this.areaIndicatorInstance)
			{
				EntityState.Destroy(this.areaIndicatorInstance.gameObject);
			}		
		}

        public override InterruptPriority GetMinimumInterruptPriority()
        {
			return InterruptPriority.Skill;
        }
    }
}
