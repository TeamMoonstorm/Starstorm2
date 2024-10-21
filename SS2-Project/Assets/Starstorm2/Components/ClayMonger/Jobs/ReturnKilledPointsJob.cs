using SS2.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct ReturnKilledPointsJob : IJobParallelForDefer
    {
        [WriteOnly, NativeDisableParallelForRestriction]
        public NativeArray<MongerTarTrailManager.TarPoint> allPoints;
        [ReadOnly]
        public NativeArray<MongerTarTrailManager.TarPoint> killedPointsJob;
        public NativeQueue<int>.ParallelWriter invalidPointIndices;
        public void Execute(int index)
        {
            var killedPoint = killedPointsJob[index];
            int managerIndex = (int)killedPoint.managerIndex;
            invalidPointIndices.Enqueue((int)killedPoint.managerIndex);
            allPoints[managerIndex] = MongerTarTrailManager.TarPoint.invalid;
        }
    }
}