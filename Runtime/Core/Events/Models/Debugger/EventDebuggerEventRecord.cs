using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
namespace Kurisu.Framework.Events
{
    [Serializable]
    internal class EventDebuggerRecordList
    {
        public List<EventDebuggerEventRecord> eventList;
    }
    /// <summary>
    /// Control <see cref="JsonConverter"/> used in debugger, used when json convertors for unity not installed.
    /// Recommend to use https://github.com/applejag/Newtonsoft.Json-for-Unity.Converters
    /// </summary>
    public static class DebuggerConverterSettings
    {
        public static JsonConverter[] Converters { get; set; }
    }
    [Serializable]
    internal class EventDebuggerEventRecord
    {
        [field: SerializeField]
        public string EventBaseName { get; private set; }
        [field: SerializeField]
        public long EventTypeId { get; private set; }
        [field: SerializeField]
        public string EventType { get; private set; }
        [field: SerializeField]
        public ulong EventId { get; private set; }
        [field: SerializeField]
        internal long Timestamp { get; private set; }
        public IEventHandler Target { get; set; }
        public PropagationPhase PropagationPhase { get; private set; }
        public string JsonData { get; set; }
        void Init(EventBase evt)
        {
            var type = evt.GetType();
            EventBaseName = EventDebugger.GetTypeDisplayName(type);
            EventType = type.AssemblyQualifiedName;
            EventTypeId = evt.EventTypeId;
            EventId = evt.EventId;
            Timestamp = evt.Timestamp;
            Target = evt.Target;
            PropagationPhase = evt.PropagationPhase;
#if JSON_CONVERTERS_FOR_UNITY_INSTALL
            JsonData = JsonConvert.SerializeObject(evt);
#else
            JsonData = JsonConvert.SerializeObject(evt, DebuggerConverterSettings.Converters);
#endif
        }

        public EventDebuggerEventRecord(EventBase evt)
        {
            Init(evt);
        }

        public string TimestampString()
        {
            long ticks = (long)(Timestamp / 1000f * TimeSpan.TicksPerSecond);
            return new DateTime(ticks).ToString("HH:mm:ss.ffffff");
        }
    }
}