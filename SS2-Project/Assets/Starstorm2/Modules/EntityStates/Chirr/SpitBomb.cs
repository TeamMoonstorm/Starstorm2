using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using RoR2;
using RoR2.Projectile;
namespace EntityStates.Chirr
{
    public class SpitBomb : AimThrowableBase
    {
        private static GameObject INDICATOR = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BasicThrowableVisualizer.prefab").WaitForCompletion();

        private static float maxDistanceee = 30f;
        public override void OnEnter()
        {

            this.arcVisualizerPrefab = INDICATOR;
            this.endpointVisualizerPrefab = Huntress.ArrowRain.areaIndicatorPrefab;
            this.rayRadius = 0.5f;
            this.useGravity = true;
            this.maxDistance = maxDistanceee;
            base.OnEnter();
            //Util.PlaySound();
            //base.PlayAnimation();
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
