using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using SS2.Components;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct WriteBoxcastCommandsJob : IJobParallelFor
    {
        public PhysicsScene physicsScene;
        public LayerMask physicsCheckMask;
        public NativeArray<MongerTarTrailManager.TarPoint> tarPoints;
        public NativeArray<BoxcastCommand> output;

        //index is equal to tarPoints[index]'s managerIndex.
        public void Execute(int index)
        {
            var tarPoint = tarPoints[index];

            var xy = tarPoint.pointWidthDepth * tarPoint.remappedLifetime0to1;
            var command = new BoxcastCommand(physicsScene, tarPoint.worldPosition, new float3(xy.x / 2, 0.5f, xy.y / 2), tarPoint.rotation, tarPoint.normalDirection, 1, physicsCheckMask);

            output[index] = command;
        }
    }
}