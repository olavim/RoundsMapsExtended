using MapsExt.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt
{
	public static class PropertyExtensions
	{
		public static IProperty ReadProperty(this GameObject mapObject, Type propertyType)
		{
			return PropertyManager.Current.Read(mapObject, propertyType);
		}

		public static T ReadProperty<T>(this GameObject mapObject) where T : IProperty
		{
			return (T) mapObject.ReadProperty(typeof(T));
		}

		public static IEnumerable<IProperty> ReadProperties(this GameObject mapObject, Type propertyType)
		{
			return PropertyManager.Current.ReadAll(mapObject, propertyType);
		}

		public static IEnumerable<T> ReadProperties<T>(this GameObject mapObject) where T : IProperty
		{
			return mapObject.ReadProperties(typeof(T)).Cast<T>();
		}

		public static bool TryWriteProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			try
			{
				mapObject.WriteProperty(prop);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static void WriteProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			PropertyManager.Current.Write(prop, mapObject);
		}

		public static T ReadProperty<T>(this Component comp) where T : IProperty
		{
			return comp.gameObject.ReadProperty<T>();
		}

		public static IProperty ReadProperty(this Component comp, Type propertyType)
		{
			return comp.gameObject.ReadProperty(propertyType);
		}

		public static void WriteProperty<T>(this Component comp, T prop) where T : IProperty
		{
			comp.gameObject.WriteProperty(prop);
		}

		public static bool TryWriteProperty<T>(this Component comp, T prop) where T : IProperty
		{
			return comp.gameObject.TryWriteProperty(prop);
		}
	}
}
