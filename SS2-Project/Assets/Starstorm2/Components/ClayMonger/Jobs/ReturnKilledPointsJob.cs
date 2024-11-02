using SS2.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct ReturnKilledPointsJob : IJobParallelFor
    {
        public NativeArray<MongerTarTrailManager.TarPoint> allPoints;
        public NativeArray<MongerTarTrailManager.TarPoint> killedPoints;
        public NativeQueue<int>.ParallelWriter invalidPointIndices;
        public void Execute(int index)
        {
            if (index >= killedPoints.Length)
                return;

            var killedPoint = killedPoints[index];
            int managerIndex = (int)killedPoint.managerIndex;
            invalidPointIndices.Enqueue((int)killedPoint.managerIndex);
            allPoints[managerIndex] = MongerTarTrailManager.TarPoint.invalid;
        }
    }
}