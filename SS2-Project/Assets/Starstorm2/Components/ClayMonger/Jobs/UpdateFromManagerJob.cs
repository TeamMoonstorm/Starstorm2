using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using SS2.Components;

namespace SS2.Jobs
{

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct UpdateFromManagerJob : IJobFor
    {
        [ReadOnly]
        public NativeList<MongerTarTrailManager.TarPoint> allTarPoints;
        public NativeArray<MongerTarTrailManager.TarPoint> myTarPoints;
        public void Execute(int index)
        {
            var myPoint = myTarPoints[index];
            if (!myPoint.isValid)
                return;

            myTarPoints[index] = allTarPoints[(int)myPoint.managerIndex];
        }
    }
}