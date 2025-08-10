using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace SS2.Jobs
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public struct WriteRaycastCommandsJob : IJobParallelForTransform
    {
        public float raycastLength;
        public PhysicsScene physicsScene;
        public LayerMask raycastMask;
        public NativeArray<RaycastCommand> output;

        public void Execute(int index, TransformAccess access)
        {
            output[index] = new RaycastCommand(physicsScene, access.position, math.down(), raycastLength, raycastMask, 1);
        }
    }
}