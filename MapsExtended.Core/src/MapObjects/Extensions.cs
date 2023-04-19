using MapsExt.MapObjects;
using System;
using UnityEngine;

namespace MapsExt.Properties
{
	public static class PropertyExtensions
	{
		public static T GetMapObjectProperty<T>(this GameObject mapObject) where T : IProperty
		{
			return (T) mapObject.GetMapObjectProperty(typeof(T));
		}

		public static IProperty GetMapObjectProperty(this GameObject mapObject, Type propertyType)
		{
			var dataType = mapObject.GetComponent<MapObjectInstance>().DataType;

			if (MapsExtended.PropertyManager.GetSerializableMember(dataType, propertyType) == null)
			{
				return default;
			}

			return MapsExtended.PropertyManager.GetSerializer(propertyType)?.Serialize(mapObject);
		}

		public static void SetMapObjectProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			var dataType = mapObject.GetComponent<MapObjectInstance>().DataType;

			if (MapsExtended.PropertyManager.GetSerializableMember(dataType, prop.GetType()) == null)
			{
				throw new ArgumentException($"{dataType.Name} does not have property {prop.GetType().Name}");
			}

			MapsExtended.PropertyManager.GetSerializer(prop.GetType()).Deserialize(prop, mapObject);
		}

		public static bool TrySetMapObjectProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			try
			{
				mapObject.SetMapObjectProperty(prop);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static T GetMapObjectProperty<T>(this Component comp) where T : IProperty
		{
			return comp.gameObject.GetMapObjectProperty<T>();
		}

		public static IProperty GetMapObjectProperty(this Component comp, Type propertyType)
		{
			return comp.gameObject.GetMapObjectProperty(propertyType);
		}

		public static void SetMapObjectProperty<T>(this Component comp, T prop) where T : IProperty
		{
			comp.gameObject.SetMapObjectProperty(prop);
		}

		public static bool TrySetMapObjectProperty<T>(this Component comp, T prop) where T : IProperty
		{
			return comp.gameObject.TrySetMapObjectProperty(prop);
		}
	}
}
