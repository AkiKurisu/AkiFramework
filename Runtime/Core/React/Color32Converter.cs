using System;
using Newtonsoft.Json;
using UnityEngine;
namespace Kurisu.Framework.React
{
    /// <summary>
    /// Use built in serialization for color type
    /// </summary>
    public class Color32Converter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType)
        {
            return typeof(Color32) == objectType;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return JsonUtility.FromJson<Color32>(reader.ReadAsString());
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, JsonUtility.ToJson(value));
        }
    }
}