using Ceres.Annotations;
using Ceres.Graph.Flow;
using Ceres.Graph.Flow.Annotations;
using Ceres.Graph.Flow.Utilities;
using Chris.Schedulers;
using UnityEngine.Scripting;
using UObject = UnityEngine.Object;
namespace Chris.Ceres
{
    [Preserve]
    public class ChrisExecutableFunctionLibrary: ExecutableFunctionLibrary
    {
        [ExecutableFunction, CeresLabel("Schedule Timer by Event")]
        public static SchedulerHandle Flow_SchedulerDelay([CeresMetadata(CeresMetadata.SELF_TARGET)] UObject context, 
            float delaySeconds, EventDelegate<float> onUpdate, EventDelegate onComplete)
        {
            var handle = Scheduler.Delay(delaySeconds, onUpdate: onUpdate?.Create(context), onComplete: onComplete?.Create(context));
            return handle;
        }
        
        [ExecutableFunction, CeresLabel("Schedule FrameCounter by Event")]
        public static SchedulerHandle Flow_SchedulerWaitFrame([CeresMetadata(CeresMetadata.SELF_TARGET)] UObject context, 
            int frame, EventDelegate<int> onUpdate, EventDelegate onComplete)
        {
            var handle = Scheduler.WaitFrame(frame, onUpdate: onUpdate?.Create(context), onComplete: onComplete?.Create(context));
            return handle;
        }
        
        [ExecutableFunction(IsScriptMethod = true), CeresLabel("Cancel Scheduler")]
        public static void Flow_SchedulerHandleCancel(SchedulerHandle handle)
        {
            handle.Cancel();
        }
    }
}
