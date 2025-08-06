using SS2.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct TrailPointVisualJob : IJobParallelForTransform
    {
        public float totalLifetime;
        public float3 maxSize;
        [ReadOnly]
        public NativeArray<MongerTarTrailManager.TarPoint> tarPoints;

        public void Execute(int index, TransformAccess transform)
        {
            var point = tarPoints[index];
            if (!transform.isValid || !point.isValid)
            {
                return;
            }

            transform.localScale = math.lerp(float3.zero, maxSize, point.remappedLifetime0to1);
        }
    }

}