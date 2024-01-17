using System;
using System.Reflection;
using UnityEditor;
namespace Kurisu.Framework.Editor
{
	public static class ManagedReferenceUtility
	{

		public static object SetManagedReference(this SerializedProperty property, Type type)
		{
			object obj = (type != null) ? Activator.CreateInstance(type) : null;
			property.managedReferenceValue = obj;
			return obj;
		}

		public static Type GetType(string typeName)
		{
			int splitIndex = typeName.IndexOf(' ');
			var assembly = Assembly.Load(typeName[..splitIndex]);
			return assembly.GetType(typeName[(splitIndex + 1)..]);
		}

	}
}