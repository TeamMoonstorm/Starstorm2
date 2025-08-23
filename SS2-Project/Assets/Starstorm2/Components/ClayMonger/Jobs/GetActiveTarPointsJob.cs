using SS2.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct GetActiveTarPointsJob : IJobParallelFor
    {
        public NativeArray<MongerTarTrailManager.TarPoint> allTarPoints;
        public NativeList<MongerTarTrailManager.ManagerIndex>.ParallelWriter activePoints;
        public void Execute(int index)
        {
            var point = allTarPoints[index];
            if (point.isValid)
            {
                activePoints.AddNoResize(point.managerIndex);
            }
        }
    }
}