using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace EntityStates.Chirr
{
	public class SpitBombFuse : AimThrowableBase
	{
        private static GameObject INDICATOR = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();

        [NonSerialized]
        public static float maxDistanceee = 30f; // FUCK ESCS FUCK RTHE EDITOR
        public override void OnEnter()
        {

            this.arcVisualizerPrefab = INDICATOR;
            this.endpointVisualizerPrefab = Huntress.ArrowRain.areaIndicatorPrefab;
            this.rayRadius = 0.5f;
            this.useGravity = true;
            this.maxDistance = maxDistanceee;
            this.minimumDuration = 0.15f;
            this.baseMinimumDuration = 0.15f;
            this.setFuse = true;
            base.StartAimMode();
            base.OnEnter();
            //Util.PlaySound();
            //base.PlayAnimation();
        }

        public override void FireProjectile()
        {
            base.FireProjectile();
			Util.PlaySound("Play_nemmerc_knife_throw", base.gameObject);
		}

		public override InterruptPriority GetMinimumInterruptPriority()
		{
			return InterruptPriority.PrioritySkill;
		}
	}
}
