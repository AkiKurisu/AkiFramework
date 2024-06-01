using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        internal static readonly Dictionary<IScheduled, ListenerRecord> s_Listeners = new();
        public static void CleanListeners()
        {
            s_Listeners.Clear();
        }
        public static void RegisterListener(IScheduled scheduled, Delegate callback)
        {
            var declType = callback.Method.DeclaringType?.Name ?? string.Empty;
            string objectName = callback.Target.ToString();
            string itemName = declType + "." + callback.Method.Name + " > " + " [" + objectName + "]";
            StackFrame callStack = new(4, true);
            s_Listeners.Add(scheduled, new ListenerRecord
            {
                hashCode = callback.GetHashCode(),
                name = itemName,
                fileName = callStack.GetFileName(),
                lineNumber = callStack.GetFileLineNumber()
            });
        }
        public static bool TryGetListener(IScheduled scheduled, out ListenerRecord record)
        {
            return s_Listeners.TryGetValue(scheduled, out record);
        }
        public static void UnregisterListener(IScheduled scheduled, Delegate callback)
        {
            if (!s_Listeners.TryGetValue(scheduled, out ListenerRecord record))
                return;

            var declType = callback.Method.DeclaringType?.Name ?? string.Empty;
            string objectName = callback.Target.ToString();
            var itemName = declType + "." + callback.Method.Name + " > " + " [" + objectName + "]";

            if (record.name == itemName)
            {
                s_Listeners.Remove(scheduled);
            }
        }
    }
}