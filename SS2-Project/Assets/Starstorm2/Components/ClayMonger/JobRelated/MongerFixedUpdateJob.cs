using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

namespace SS2.Jobs
{
    public struct MongerFixedUpdateJob : IJobParallelFor
    {
        public float deltaTime;
        public NativeArray<MongerData> datas;
        public void Execute(int index)
        {
            var data = datas[index];
            data.trailUpdateStopwatch += deltaTime;
            data.trailCheckStopwatch += deltaTime;
            datas[index] = data;
        }

        public struct MongerData
        {
            public float trailUpdateStopwatch;
            public float trailCheckStopwatch;

        }
    }
}