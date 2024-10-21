using SS2.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct KillPointsJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<MongerTarTrailManager.TarPoint> points;
        [WriteOnly]
        public NativeList<MongerTarTrailManager.TarPoint>.ParallelWriter pointsToKill;

        public void Execute(int index)
        {
            var point = points[index];
            if (point.pointLifetime <= 0)
            {
                pointsToKill.AddNoResize(point);
            }
        }
    }
}