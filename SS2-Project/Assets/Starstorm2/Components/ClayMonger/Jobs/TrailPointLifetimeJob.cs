using SS2.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct TrailPointLifetimeJob : IJobParallelFor
    {
        public float deltaTime;
        public NativeArray<MongerTarTrailManager.TarPoint> tarPoints;

        public void Execute(int index)
        {
            var point = tarPoints[index];
            if (!point.isValid)
                return;

            point.pointLifetime -= deltaTime;
            point.remappedLifetime0to1 = math.clamp(math.remap(0f, point.totalLifetime, 0, 1, point.pointLifetime), 0, 1);
            tarPoints[index] = point;
        }
    }
}