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
	public class FireTurretTeleporter : AimThrowableBase
	{
		public static GameObject projectilePrefab;
		public static string soundString = "Play_engi_M2_throw";
		public static GameObject muzzleEffectPrefab;

		private static GameObject INDICATOR = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();


		private static float maxDistanceee = 15f;

		//stupid
		public override void OnEnter()
		{
			this.arcVisualizerPrefab = INDICATOR;
			this.endpointVisualizerPrefab = Huntress.ArrowRain.areaIndicatorPrefab;
			this.rayRadius = 0.5f;
			this.useGravity = true;
			this.endpointVisualizerRadiusScale = 0.5f;
			this.maxDistance = maxDistanceee;
			base.projectilePrefab = FireTurretTeleporter.projectilePrefab;
			this.damageCoefficient = 0f;
			this.baseMinimumDuration = 0.15f;

			base.OnEnter();
			StartAimMode();
			//anim
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
