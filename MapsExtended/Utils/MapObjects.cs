using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using System;

namespace MapsExt
{
	public static class MapObjectUtils
	{
		public static Type GetMapObjectPropertySerializerTargetType(Type mapObjectSerializerType)
		{
			foreach (Type interfaceType in mapObjectSerializerType.GetInterfaces())
			{
				if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IPropertySerializer<>))
				{
					return interfaceType.GetGenericArguments()[0];
				}
			}

			return null;
		}

		public static Type GetMapObjectDataType(Type mapObjectType)
		{
			foreach (Type interfaceType in mapObjectType.GetInterfaces())
			{
				if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == typeof(IMapObject<>))
				{
					return interfaceType.GetGenericArguments()[0];
				}
			}

			return null;
		}
	}
}
