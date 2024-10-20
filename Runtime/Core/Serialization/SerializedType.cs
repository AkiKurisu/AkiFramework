using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
namespace Kurisu.Framework.Serialization
{
    // Modified from Unity
    public static class SerializedType
    {
        #region Private helpers
        private struct SerializedTypeData
        {
            public string typeName;
            public string genericTypeName;
            public bool isGeneric;
        }
        private static string StripTypeNameString(string str, int index)
        {
            int toIndex = index + 1;
            while (toIndex < str.Length && str[toIndex] != ',' && str[toIndex] != ']')
                toIndex++;
            return str.Remove(index, toIndex - index);
        }

        private static string StripAllFromTypeNameString(string str, string toStrip)
        {
            int lastIndex = str.IndexOf(toStrip);
            while (lastIndex != -1)
            {
                str = StripTypeNameString(str, lastIndex);
                lastIndex = str.IndexOf(toStrip, lastIndex);
            }
            return str;
        }

        private static string ToShortTypeName(Type t)
        {
            // strip version, token and culture info, only leave type name and assembly name
            var name = t.AssemblyQualifiedName;
            if (string.IsNullOrEmpty(name))
                return string.Empty;
            name = StripAllFromTypeNameString(name, ", Version");
            name = StripAllFromTypeNameString(name, ", Culture");
            name = StripAllFromTypeNameString(name, ", PublicKeyToken");
            return name;
        }

        private static string SafeTypeName(Type type)
        {
            return type.FullName?.Replace('+', '.');
        }

        private static SerializedTypeData SplitTypeString(string serializedTypeString)
        {
            if (string.IsNullOrEmpty(serializedTypeString))
                throw new ArgumentException("Cannot parse serialized type string, it is empty.");

            SerializedTypeData data;
            data.isGeneric = IsGeneric(serializedTypeString);
            data.typeName = serializedTypeString.Substring(0, serializedTypeString.IndexOf('#'));
            data.genericTypeName = serializedTypeString.Substring(data.typeName.Length + 1,
                serializedTypeString.IndexOf('#', data.typeName.Length + 1) - data.typeName.Length - 1);
            return data;
        }

        private static string ToString(SerializedTypeData data)
        {
            return data.typeName + "#" + data.genericTypeName + "#" + (data.isGeneric ? "1" : "0");
        }

        private static Type FromString(SerializedTypeData data)
        {
            return Type.GetType(data.typeName, true);
        }

        #endregion

        #region Public Helpers
        public static Type GenericType(Type t)
        {
            if (t.IsArray)
                return t.GetElementType();
            if (!t.IsGenericType)
                return t;
            var args = t.GetGenericArguments();
            if (args.Length != 1)
                throw new ArgumentException("Internal error: got generic type with more than one generic argument.");
            return args[0];
        }

        public static bool IsListType(Type t)
        {
            return typeof(IList).IsAssignableFrom(t);
        }

        public static string GetFullName(Type t)
        {
            if (!t.IsGenericType)
                return SafeTypeName(t);
            if (t.GetGenericTypeDefinition() != typeof(List<>))
                throw new ArgumentException("Internal error: got unsupported generic type");
            return string.Format("System.Collections.Generic.List<{0}>", SafeTypeName(t.GetGenericArguments()[0]));
        }

        #endregion

        public static string ToString(Type t)
        {
            var data = new SerializedTypeData();

            if (t == null)
                return string.Empty;

            data.typeName = string.Empty;
            data.isGeneric = t.ContainsGenericParameters;

            if (data.isGeneric && t.IsGenericType)
                data.typeName = ToShortTypeName(t.GetGenericTypeDefinition());
            else if (data.isGeneric && t.IsArray)
                data.typeName = "T[]";
            else if (data.isGeneric)
                data.typeName = "T";
            else
                data.typeName = ToShortTypeName(t);

            return ToString(data);
        }

        public static Type FromString(string serializedTypeString)
        {
            if (SerializedTypeRedirector.TryRedirect(serializedTypeString, out var type))
            {
                return type;
            }
            if (string.IsNullOrEmpty(serializedTypeString) || IsGeneric(serializedTypeString))
                return null;
            var data = SplitTypeString(serializedTypeString);
            return Type.GetType(data.typeName);
        }

        public static bool IsGeneric(string serializedTypeString)
        {
            if (string.IsNullOrEmpty(serializedTypeString))
                return false;
            return serializedTypeString[^1] == '1';
        }

        public static bool IsBaseTypeGeneric(string serializedTypeString)
        {
            if (string.IsNullOrEmpty(serializedTypeString))
                return false;
            var data = SplitTypeString(serializedTypeString);
            return data.isGeneric || data.genericTypeName != string.Empty;
        }

        public static string SetGenericArgumentType(string serializedTypeString, Type type)
        {
            if (!IsGeneric(serializedTypeString))
            {
                if (IsBaseTypeGeneric(serializedTypeString))
                    throw new ArgumentException("Trying to set a different generic type. Reset old one first.");

                throw new ArgumentException("Trying to set generic argument type for non generic type.");
            }

            var data = SplitTypeString(serializedTypeString);

            data.genericTypeName = data.typeName;
            data.isGeneric = false;

            data.typeName = data.typeName switch
            {
                "T" => ToShortTypeName(type),
                "T[]" => ToShortTypeName(type.MakeArrayType()),
                _ => ToShortTypeName(Type.GetType(data.typeName, true).GetGenericTypeDefinition().MakeGenericType(type)),
            };
            return ToString(data);
        }

        public static string ResetGenericArgumentType(string serializedTypeString)
        {
            if (string.IsNullOrEmpty(serializedTypeString))
                throw new ArgumentException("Cannot reset generic argument type for null type.");

            var data = SplitTypeString(serializedTypeString);

            if (string.IsNullOrEmpty(data.genericTypeName))
                throw new ArgumentException("Cannot reset generic argument type, previous generic type unknown.");

            data.typeName = data.genericTypeName;
            data.isGeneric = true;
            data.genericTypeName = string.Empty;

            return ToString(data);
        }

        public static bool CanAssignFromGenericType(string serializedTypeString, Type t)
        {
            var data = SplitTypeString(serializedTypeString);
            if (!data.isGeneric)
                return false;

            if (t.IsGenericType)
            {
                // don't allow connecting e.g. List<> to T (which is assumed to be simple non-generic type)
                if (data.typeName == "T" || data.typeName == "T[]")
                    return false;

                var args = t.GetGenericArguments();
                if (args.Length != 1)
                    return false;

                if (args[0].IsGenericType)
                    return false;

                return t.GetGenericTypeDefinition() == FromString(data).GetGenericTypeDefinition();
            }

            return data.typeName == "T" || data.typeName == "T[]"; // no constraints right now
        }
    }
    /// <summary>
    /// Serialized type that will serialize metadata of class implementing T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [Serializable]
    public sealed class SerializedType<T>
    {
        /// <summary>
        /// Formatted type metadata, see <see cref="SerializedType"/>
        /// </summary>
        public string serializedTypeString;
#pragma warning disable CS8632
        private T? value;
#pragma warning restore CS8632
        /// <summary>
        /// Get default object from <see cref="SerializedType{T}"/>
        /// </summary>
        /// <returns></returns>
        public T GetObject()
        {
            if (value == null)
            {
                var type = SerializedType.FromString(serializedTypeString);
                if (type != null)
                {
                    value = (T)Activator.CreateInstance(type);
                }
            }
            return value;
        }
        /// <summary>
        /// Get object type from <see cref="SerializedType{T}"/>
        /// </summary>
        /// <returns></returns>
        public Type GetObjectType()
        {
            if (value != null)
            {
                return value.GetType();
            }
            return SerializedType.FromString(serializedTypeString);
        }
        /// <summary>
        /// Create <see cref="SerializedType{T}"/> from type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static SerializedType<T> FromType(Type type)
        {
            return new SerializedType<T>()
            {
                serializedTypeString = SerializedType.ToString(type)
            };
        }
        internal void InternalUpdate()
        {
            if (value != null && SerializedType.ToString(value.GetType()) != serializedTypeString)
            {
                value = default;
            }
        }
    }
    /// <summary>
    /// Attribute type that changed assembly, namespace or class name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class FormerlySerializedTypeAttribute : Attribute
    {
        private readonly string m_oldSerializedType;

        public string OldSerializedType => m_oldSerializedType;
        public FormerlySerializedTypeAttribute(string oldSerializedType)
        {
            m_oldSerializedType = oldSerializedType;
        }
    }
    /// <summary>
    /// Attribute type that will not show in SerializedType search window
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class HideInSerializedTypeAttribute : Attribute { }
    public static class SerializedTypeRedirector
    {
        private static readonly Lazy<Dictionary<string, Type>> updatableType;
        static SerializedTypeRedirector()
        {
            updatableType = new Lazy<Dictionary<string, Type>>(() =>
            {
                return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                                                    .Where(x => x.GetCustomAttribute<FormerlySerializedTypeAttribute>() != null)
                                                    .ToDictionary(x => x.GetCustomAttribute<FormerlySerializedTypeAttribute>().OldSerializedType, x => x);
            });
        }
        /// <summary>
        /// Try get redirected type
        /// </summary>
        /// <param name="stringType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool TryRedirect(string stringType, out Type type)
        {
            return updatableType.Value.TryGetValue(stringType, out type);
        }
    }
}
