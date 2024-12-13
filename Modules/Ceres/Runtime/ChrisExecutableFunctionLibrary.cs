using Ceres.Annotations;
using Ceres.Graph.Flow;
using Ceres.Graph.Flow.Annotations;
using Ceres.Graph.Flow.Utilities;
using Chris.Schedulers;
using UnityEngine.Assertions;
using UnityEngine.Scripting;
using UObject = UnityEngine.Object;
namespace Chris.Ceres
{
    [Preserve]
    public class ChrisExecutableFunctionLibrary: ExecutableFunctionLibrary
    {
        [ExecutableFunction]
        public static SchedulerHandle Flow_SetTimer([CeresMetadata(CeresMetadata.SELF_TARGET)] UObject context, 
            float delaySeconds, EventDelegate eventDelegate)
        {
            Assert.IsTrue(eventDelegate.IsValid());
            var handle = Scheduler.Delay(delaySeconds, () =>
            {
                eventDelegate.Invoke(context);
            });
            return handle;
        }
        
        [ExecutableFunction]
        public static void Flow_ClearTimer(SchedulerHandle handle)
        {
            handle.Cancel();
        }
    }
}
