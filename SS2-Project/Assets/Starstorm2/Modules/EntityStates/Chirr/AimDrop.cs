﻿using UnityEngine;
using RoR2;
using SS2.Components;
using EntityStates.Huntress;
using SS2;
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

		public bool cancelled;

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
			//if(Util.CharacterRaycast(base.gameObject, new Ray(aimOrigin,aimDirection), out raycastHit, maxRayDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal))
            {
				hitPoint = raycastHit.point;
			}
			Vector3 victimFootPosition = aimOrigin;
			if (this.grabController.IsGrabbing()) victimFootPosition = GetBetterFootPosition();
			hitPoint.y = Mathf.Min(hitPoint.y - 0.1f, victimFootPosition.y - 0.1f); // if hitpoint is higher or equal to footposition, it wont be pretty
			if (this.areaIndicatorInstance)
			{
				this.areaIndicatorInstance.transform.position = hitPoint;
				this.areaIndicatorInstance.transform.up = hitPoint;
			}

			//calculate trajectory with no upwards velocity			
			float flightDuration = Trajectory.CalculateFlightDuration(victimFootPosition.y, hitPoint.y, 0f, Physics.gravity.y + extraGravity);
			if (float.IsNaN(flightDuration) || flightDuration < 0.2f) flightDuration = .2f; // hopefully final crash fix
			Vector3 hBetween = hitPoint - aimOrigin;
			hBetween.y = 0;
			float hDistance = hBetween.magnitude;
			float hSpeed = hDistance / flightDuration;
			
			this.desiredTrajectory = hSpeed * hBetween.normalized;


			SS2Log.Info("aimOrigin " + aimOrigin);
			SS2Log.Info("hitPoint " + hitPoint);
			SS2Log.Info("victimFootPosition " + victimFootPosition);
			SS2Log.Info("flightDuration " + flightDuration);
			SS2Log.Info("hBetween " + hBetween);
			SS2Log.Info("hSpeed " + hSpeed);
			SS2Log.Info("hDistance " + hDistance);
			SS2Log.Info("desiredTrajectory " + desiredTrajectory);
		}

		public Vector3 GetBetterFootPosition()
		{
			Vector3 position = this.grabController.victimBodyObject.transform.position;
			if (this.grabController.victimInfo.characterMotor)
			{
				position = this.grabController.victimInfo.body.footPosition;
				return position;
			}
			if (this.grabController.victimColliders.Length > 0) // lowest point on collider (assuming its centered)
			{
				position.y = this.grabController.victimColliders[0].bounds.min.y;
				return position;
			}
			return position;
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

            if (!this.cancelled && !this.outer.destroying && this.grabController && this.grabController.IsGrabbing())
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
