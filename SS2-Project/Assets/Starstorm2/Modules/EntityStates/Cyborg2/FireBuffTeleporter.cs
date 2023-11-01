using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEngine;
using RoR2.Projectile;
using UnityEngine.AddressableAssets;
namespace EntityStates.Cyborg2
{
	public class FireBuffTeleporter : AimThrowableBase
	{

		public static GameObject projectilePrefab;
		public static string soundString = "Play_engi_M2_throw";
		public static GameObject muzzleEffectPrefab;

		private static GameObject INDICATOR = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();

		// im so idiot lazy
		private static float maxDistanceee = 15f;


		private float antiGravityCoefficient = 1;
		//stupid
		public override void OnEnter()
		{
			this.arcVisualizerPrefab = INDICATOR;
			this.endpointVisualizerPrefab = Huntress.ArrowRain.areaIndicatorPrefab;
			this.rayRadius = 0.5f;
			this.useGravity = true;
			this.endpointVisualizerRadiusScale = 0.5f;
			this.maxDistance = maxDistanceee;
			base.projectilePrefab = FireBuffTeleporter.projectilePrefab;
			this.damageCoefficient = 0f;
			this.baseMinimumDuration = 0.15f;

			AntiGravityForce antiGravityForce = base.projectilePrefab.GetComponent<AntiGravityForce>();
			if (antiGravityForce)
			{
				antiGravityCoefficient = antiGravityForce.antiGravityCoefficient;
			}

			base.OnEnter();
			StartAimMode();
			//anim
		}

		public override void UpdateTrajectoryInfo(out AimThrowableBase.TrajectoryInfo dest)
		{
			dest = default(AimThrowableBase.TrajectoryInfo);
			Ray aimRay = base.GetAimRay();
			RaycastHit raycastHit = default(RaycastHit);
			bool flag = false;
			if (this.rayRadius > 0f && Util.CharacterSpherecast(base.gameObject, aimRay, this.rayRadius, out raycastHit, this.maxDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal) && raycastHit.collider.GetComponent<HurtBox>())
			{
				flag = true;
			}
			if (!flag)
			{
				flag = Util.CharacterRaycast(base.gameObject, aimRay, out raycastHit, this.maxDistance, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.UseGlobal);
			}
			if (flag)
			{
				dest.hitPoint = raycastHit.point;
				dest.hitNormal = raycastHit.normal;
			}
			else
			{
				dest.hitPoint = aimRay.GetPoint(this.maxDistance);
				dest.hitNormal = -aimRay.direction;
			}
			Vector3 vector = dest.hitPoint - aimRay.origin;
			if (this.useGravity)
			{
				float num = this.projectileBaseSpeed;
				Vector2 vector2 = new Vector2(vector.x, vector.z);
				float magnitude = vector2.magnitude;
				
				float y = Trajectory.CalculateInitialYSpeed(magnitude / num, vector.y, this.antiGravityCoefficient * Physics.gravity.y);
				Vector3 a = new Vector3(vector2.x / magnitude * num, y, vector2.y / magnitude * num);
				dest.speedOverride = a.magnitude;
				dest.finalRay = new Ray(aimRay.origin, a / dest.speedOverride);
				dest.travelTime = Trajectory.CalculateGroundTravelTime(num, magnitude);
				return;
			}
			dest.speedOverride = this.projectileBaseSpeed;
			dest.finalRay = aimRay;
			dest.travelTime = this.projectileBaseSpeed / vector.magnitude;
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
