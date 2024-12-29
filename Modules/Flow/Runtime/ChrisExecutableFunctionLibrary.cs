using Ceres.Annotations;
using Ceres.Graph.Flow;
using Ceres.Graph.Flow.Annotations;
using Ceres.Graph.Flow.Utilities;
using Chris.Schedulers;
using Chris.Serialization;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Scripting;
using UObject = UnityEngine.Object;
namespace Chris.Flow
{
    /// <summary>
    /// Executable function library for Chris
    /// </summary>
    [Preserve]
    public class ChrisExecutableFunctionLibrary: ExecutableFunctionLibrary
    {
        #region Scheduler

        [ExecutableFunction, CeresLabel("Schedule Timer by Event")]
        public static SchedulerHandle Flow_SchedulerDelay(
            [CeresMetadata(CeresMetadata.SELF_TARGET)] UObject context, 
            float delaySeconds, EventDelegate<float> onUpdate, EventDelegate onComplete)
        {
            var handle = Scheduler.Delay(delaySeconds, onUpdate: onUpdate?.Create(context), onComplete: onComplete?.Create(context));
            return handle;
        }
        
        [ExecutableFunction, CeresLabel("Schedule FrameCounter by Event")]
        public static SchedulerHandle Flow_SchedulerWaitFrame(
            [CeresMetadata(CeresMetadata.SELF_TARGET)] UObject context, 
            int frame, EventDelegate<int> onUpdate, EventDelegate onComplete)
        {
            var handle = Scheduler.WaitFrame(frame, onUpdate: onUpdate?.Create(context), onComplete: onComplete?.Create(context));
            return handle;
        }
        
        [ExecutableFunction(IsScriptMethod = true, DisplayTarget = false), CeresLabel("Cancel Scheduler")]
        public static void Flow_SchedulerHandleCancel(SchedulerHandle handle)
        {
            handle.Cancel();
        }
        
        [ExecutableFunction(IsScriptMethod = true), CeresLabel("To Scheduler Handle")]
        public static unsafe SchedulerHandle Flow_DoubleToSchedulerHandle(
            [CeresMetadata(CeresMetadata.SELF_TARGET)] double doubleValue)
        {
            UnsafeUtility.CopyPtrToStructure(&doubleValue, out ulong handle);
            return new SchedulerHandle(handle);
        }
        
        [ExecutableFunction(IsScriptMethod = true), CeresLabel("To Double")]
        public static unsafe double Flow_SchedulerHandleToDouble(
            [CeresMetadata(CeresMetadata.SELF_TARGET)] SchedulerHandle schedulerHandle)
        {
            ulong handle = schedulerHandle.Handle;
            UnsafeUtility.CopyPtrToStructure(&handle, out double doubleValue);
            return doubleValue;
        }
        
        #endregion Scheduler

        #region Subsystem

        [ExecutableFunction]
        public static SubsystemBase Flow_GetSubsystem(
            [CeresMetadata(CeresMetadata.RESOVLE_RETURN)] SerializedType<SubsystemBase> type)
        {
            return GameWorld.Get().GetSubsystem(type);
        }

        #endregion
    }
}
