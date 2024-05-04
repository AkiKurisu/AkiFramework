using System;
using System.Collections.Generic;
using Kurisu.Framework.React;
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
    /// Control jsonConverter used in debugger
    /// </summary>
    public static class DebuggerConverterSettings
    {
        static DebuggerConverterSettings()
        {
            Converters = new JsonConverter[] { new VectorConverter() };
        }
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
            EventTypeId = evt.EventTypeId;
            EventId = evt.EventId;
            Timestamp = evt.Timestamp;
            Target = evt.Target;
            PropagationPhase = evt.PropagationPhase;
            JsonData = JsonConvert.SerializeObject(evt, DebuggerConverterSettings.Converters);
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