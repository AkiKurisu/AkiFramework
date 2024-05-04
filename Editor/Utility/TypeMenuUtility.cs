using System;
using System.Linq;
using System.Collections.Generic;
namespace Kurisu.Framework.Editor
{
	public static class TypeMenuUtility
	{

		public const string k_NullDisplayName = "<Null>";

		public static AddTypeMenu GetAttribute(Type type)
		{
			return Attribute.GetCustomAttribute(type, typeof(AddTypeMenu)) as AddTypeMenu;
		}

		public static string[] GetSplittedTypePath(Type type)
		{
			AddTypeMenu typeMenu = GetAttribute(type);
			if (typeMenu != null)
			{
				return typeMenu.GetSplittedMenuName();
			}
			else return new string[] { type.Name };
		}

		public static IEnumerable<Type> OrderByType(this IEnumerable<Type> source)
		{
			return source.OrderBy(type =>
			{
				if (type == null)
				{
					return -999;
				}
				return GetAttribute(type)?.Order ?? 0;
			}).ThenBy(type =>
			{
				if (type == null)
				{
					return null;
				}
				return GetAttribute(type)?.MenuName ?? type.Name;
			});
		}

	}
}