using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using R3;
namespace Kurisu.Framework.React
{
    /// <summary>
    /// Serialization helper when use <see cref="JsonConverter"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReactivePropertyConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ReactiveProperty<T>);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var value = token.ToObject<T>();
            return new ReactiveProperty<T>(value);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((ReactiveProperty<T>)value).Value);
        }
    }
}