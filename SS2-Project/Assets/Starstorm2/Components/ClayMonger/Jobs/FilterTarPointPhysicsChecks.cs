
using SS2.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct FilterManagerIndicesThatDidntCollideWithAnythingJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeList<MongerTarTrailManager.TarPoint> input;
        public NativeArray<RaycastHit> raycastHits;
        [WriteOnly]
        public NativeList<MongerTarTrailManager.ManagerIndex>.ParallelWriter output;

        //index is equal to tarPoints[index]'s managerIndex.
        public void Execute(int index)
        {
            if (raycastHits[index].colliderInstanceID != 0)
            {
                output.AddNoResize(input[index].managerIndex);
            }
        }
    }
}