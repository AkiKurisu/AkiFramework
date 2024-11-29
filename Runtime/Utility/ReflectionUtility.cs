using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
namespace Chris
{
    public static class ReflectionUtility
    {
         /// <summary>
        /// Get all field names of type that Unity can serialize
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<string> GetSerializedFieldsName(Type type)
        {
            List<FieldInfo> fieldInfos = new();
            GetAllSerializableFields(type, fieldInfos);
            return fieldInfos.Select(x => x.Name)
                            .ToList();
        }
         
        /// <summary>
        /// Get all fields of type that Unity can serialize
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<FieldInfo> GetSerializedFields(Type type)
        {
            List<FieldInfo> fieldInfos = new();
            GetAllSerializableFields(type, fieldInfos);
            return fieldInfos;
        }
         
        private static void GetAllSerializableFields(Type type, List<FieldInfo> fieldInfos)
        {
            if (type.BaseType != null)
            {
                GetAllSerializableFields(type.BaseType, fieldInfos);
            }

            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (FieldInfo field in fields)
            {
                if (!ValidateSerializedField(field))
                {
                    continue;
                }

                fieldInfos.Add(field);
            }
        }

        private static bool ValidateSerializedField(FieldInfo field)
        {
            if (field.IsStatic) return false;

            if (field.IsInitOnly) return false;

            if (!field.IsPublic && !Attribute.IsDefined(field, typeof(SerializeField), false)) return false;

            if (!field.FieldType.IsSerializable && !typeof(UnityEngine.Object).IsAssignableFrom(field.FieldType)
                && !field.FieldType.IsPrimitive && !field.FieldType.IsEnum
                && !typeof(List<>).IsAssignableFrom(field.FieldType) && !field.FieldType.IsArray
                && !field.FieldType.IsGenericType
                && !Attribute.IsDefined(field.FieldType, typeof(SerializableAttribute), false)
                && !IsUnityBuiltinTypes(field.FieldType))
                return false;

            return true;
        }

        public static bool IsUnityBuiltinTypes(Type type)
        {
            if (type == typeof(AnimationCurve)) return true;
            if (type == typeof(Bounds)) return true;
            if (type == typeof(BoundsInt)) return true;
            if (type == typeof(Color)) return true;
            if (type == typeof(Enum)) return true;
            if (type == typeof(Object)) return true;
            if (type == typeof(Quaternion)) return true;
            if (type == typeof(Rect)) return true;
            if (type == typeof(RectInt)) return true;
            if (type == typeof(Vector2)) return true;
            if (type == typeof(Vector2Int)) return true;
            if (type == typeof(Vector3)) return true;
            if (type == typeof(Vector3Int)) return true;
            if (type == typeof(Vector4)) return true;
            return false;
        }
    }
}
