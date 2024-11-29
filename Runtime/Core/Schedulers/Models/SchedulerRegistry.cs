using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
namespace Chris.Schedulers
{
    internal static class SchedulerRegistry
    {
        internal struct ListenerRecord
        {
            public int hashCode;
            public string name;
            public string fileName;
            public int lineNumber;
        }
        private const string MethodPtr = nameof(MethodPtr);
        private static readonly ProfilerMarker RegisterListenerPM = new("SchedulerRegistry.RegisterListener");
        private static readonly ProfilerMarker UnregisterListenerPM = new("SchedulerRegistry.UnregisterListener");
        internal static readonly Dictionary<IScheduled, ListenerRecord> s_Listeners = new();
        public static void CleanListeners()
        {
            s_Listeners.Clear();
        }
        public static void RegisterListener(IScheduled scheduled, Delegate callback)
        {
            using (RegisterListenerPM.Auto())
            {
                int hashCode = default;
                string itemName;
                if (callback == null)
                {
                    itemName = MethodPtr;
                }
                else
                {
                    hashCode = callback.GetHashCode();
                    itemName = FrameworkUtils.GetDelegatePath(callback);
                }

                StackFrame frame = FrameworkUtils.GetCurrentStackFrame();

                s_Listeners.Add(scheduled, new ListenerRecord
                {
                    hashCode = hashCode,
                    name = itemName,
                    fileName = frame.GetFileName(),
                    lineNumber = frame.GetFileLineNumber()
                });
            }
        }
        public static bool TryGetListener(IScheduled scheduled, out ListenerRecord record)
        {
            record = default;
            if (scheduled == null) return false;
            return s_Listeners.TryGetValue(scheduled, out record);
        }
        public static void UnregisterListener(IScheduled scheduled, Delegate callback)
        {
            using (UnregisterListenerPM.Auto())
            {
                if (!s_Listeners.TryGetValue(scheduled, out ListenerRecord record))
                    return;

                if (callback == null && record.name == MethodPtr)
                {
                    s_Listeners.Remove(scheduled);
                    return;
                }

                if (record.name == FrameworkUtils.GetDelegatePath(callback))
                {
                    s_Listeners.Remove(scheduled);
                }
            }
        }
    }
}