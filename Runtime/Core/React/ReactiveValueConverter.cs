using System;
using Newtonsoft.Json;
namespace Kurisu.Framework.React
{
    public class ReactiveValueConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ReactiveValue<T>).IsAssignableFrom(objectType);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            T deserializedValue = serializer.Deserialize<T>(reader);
            return (ReactiveValue<T>)Activator.CreateInstance(objectType, deserializedValue);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((ReactiveValue<T>)value).Value);
        }
    }
}