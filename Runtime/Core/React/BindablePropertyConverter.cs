using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
namespace Kurisu.Framework.React
{
    public class BindablePropertyConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(BindableProperty<T>);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var value = token.ToObject<T>();
            return new BindableProperty<T>(value);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, ((BindableProperty<T>)value).Value);
        }
    }
}