using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
using System.Linq;
using Moonstorm.Starstorm2.Components;
using EntityStates.Huntress;
using UnityEngine.Jobs;
using Unity.Jobs;
using Moonstorm.Starstorm2;

namespace EntityStates.Chirr
{
    public class AimDrop : BaseSkillState
    {
		public static float splashRadius = 10f;
		public static float damageCoefficient = 10f;
		public static float dropMaxHorizontalVelocity = 20f;
		public static float dropConeAngle = 45f;
		public static float maxRayDistance = 50f;
		private GameObject areaIndicatorInstance;

		private GrabController grabController;

		private Vector3 desiredTrajectory;

		public override void OnEnter()
        {
            base.OnEnter();

			this.grabController = base.GetComponent<GrabController>();
			if (ArrowRain.areaIndicatorPrefab)
			{
				this.areaIndicatorInstance = UnityEngine.Object.Instantiate<GameObject>(ArrowRain.areaIndicatorPrefab);
				this.areaIndicatorInstance.transform.localScale = new Vector3(ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius, ArrowRain.arrowRainRadius);
			}
		}

        public override void FixedUpdate()
        {
            base.FixedUpdate();

			if(!base.inputBank.skill3.down)//!base.IsKeyDownAuthority())
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
			// NEED TO ACCOUNT FOR VICTIM'S HEIGHT/FOOT POSITION. UNDERSHOOTS OTHERWISE
			// looking straight down, max angle can only be 45 degrees "upwards"
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
			float flightDuration = Trajectory.CalculateFlightDuration(aimOrigin.y, hitPoint.y, 0f);

			Vector3 hBetween = hitPoint - aimOrigin;
			hBetween.y = 0;
			float hDistance = hBetween.magnitude;
			float hSpeed = hDistance / flightDuration;

			this.desiredTrajectory = hSpeed * hBetween.normalized;
		}


		public override void OnExit()
		{
			base.OnExit();
			if (!this.outer.destroying && this.grabController && this.grabController.victimInfo.bodyStateMachine)
			{			
				//if (Util.HasEffectiveAuthority(this.grabController.victimBodyObject))
					
				EntityStateMachine bodyMachine = this.grabController.victimInfo.bodyStateMachine;
				
				this.grabController.AttemptGrab(null);
				bodyMachine.SetInterruptState(new DroppedState { initialVelocity = this.desiredTrajectory, inflictor = base.gameObject }, InterruptPriority.Vehicle);

				SS2Log.Warning("BITCH THROW");
			}
			if (this.areaIndicatorInstance)
			{
				EntityState.Destroy(this.areaIndicatorInstance.gameObject);
			}		
		}
	}
}
