using UnityEngine.Jobs;

namespace Heels.Job
{
    public static class HandlerJobs
    {
        public static void Recalculate()
        {
        }

        public struct HeelsUpdateJob : IJobParallelForTransform
        {
            public void Execute(int index, TransformAccess transform)
            {
            }
        }
    }
}