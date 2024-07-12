using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Profiling;
namespace Kurisu.Framework.Schedulers
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
        private const string Unmanaged = nameof(Unmanaged);
        internal static readonly Dictionary<IScheduled, ListenerRecord> s_Listeners = new();
        public static void CleanListeners()
        {
            s_Listeners.Clear();
        }
        public static void RegisterListener(IScheduled scheduled, Delegate callback)
        {
            Profiler.BeginSample("SchedulerRegistry::RegisterListener");
            int hashCode = default;
            string itemName;
            if (callback == null)
            {
                itemName = Unmanaged;
            }
            else
            {
                hashCode = callback.GetHashCode();
                itemName = Utils.GetDelegatePath(callback);
            }

            StackFrame frame = Utils.GetCurrentStackFrame();

            s_Listeners.Add(scheduled, new ListenerRecord
            {
                hashCode = hashCode,
                name = itemName,
                fileName = frame.GetFileName(),
                lineNumber = frame.GetFileLineNumber()
            });
            Profiler.EndSample();
        }
        public static bool TryGetListener(IScheduled scheduled, out ListenerRecord record)
        {
            return s_Listeners.TryGetValue(scheduled, out record);
        }
        public static void UnregisterListener(IScheduled scheduled, Delegate callback)
        {
            Profiler.BeginSample("SchedulerRegistry::UnregisterListener");
            if (!s_Listeners.TryGetValue(scheduled, out ListenerRecord record))
                return;

            if (callback == null && record.name == Unmanaged)
            {
                s_Listeners.Remove(scheduled);
                return;
            }

            if (record.name == Utils.GetDelegatePath(callback))
            {
                s_Listeners.Remove(scheduled);
            }
            Profiler.EndSample();
        }

    }
}