using Sirenix.Utilities;
using System;
using System.Linq;
using System.Reflection;

namespace MapsExt
{
	public static class ReflectionExtensions
	{
		public static object GetFieldOrPropertyValue(this MemberInfo info, object instance)
		{
			return
				info is FieldInfo field ? field.GetValue(instance) :
				info is PropertyInfo property ? property.GetValue(instance) :
				throw new ArgumentException("MemberInfo must be of type FieldInfo or PropertyInfo");
		}

		public static bool HasFieldOrProperty(this Type type, Type returnType)
		{
			const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

			return
				type.GetProperties(flags).Any(p => p.GetReturnType() == returnType) ||
				type.GetFields(flags).Any(p => p.GetReturnType() == returnType);
		}
	}
}
