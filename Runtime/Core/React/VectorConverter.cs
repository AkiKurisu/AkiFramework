using System;
using Newtonsoft.Json;
using UnityEngine;
namespace Kurisu.Framework.React
{
    /// <summary>
    /// Use built in serialization for vector type
    /// </summary>
    public class VectorConverter : JsonConverter
    {
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanConvert(Type objectType)
        {
            return typeof(Vector2) == objectType ||
            typeof(Vector2Int) == objectType ||
            typeof(Vector3) == objectType ||
            typeof(Vector3Int) == objectType ||
            typeof(Vector4) == objectType;
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return objectType switch
            {
                var t when t == typeof(Vector2) => JsonUtility.FromJson<Vector2>(reader.ReadAsString()),
                var t when t == typeof(Vector2Int) => JsonUtility.FromJson<Vector2Int>(reader.ReadAsString()),
                var t when t == typeof(Vector3) => JsonUtility.FromJson<Vector3>(reader.ReadAsString()),
                var t when t == typeof(Vector3Int) => JsonUtility.FromJson<Vector3Int>(reader.ReadAsString()),
                var t when t == typeof(Vector4) => JsonUtility.FromJson<Vector4>(reader.ReadAsString()),
                _ => throw new ArgumentOutOfRangeException(nameof(objectType)),
            };
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            switch (value)
            {
                case Vector2:
                case Vector2Int:
                case Vector3:
                case Vector3Int:
                case Vector4:
                    serializer.Serialize(writer, JsonUtility.ToJson(value));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}