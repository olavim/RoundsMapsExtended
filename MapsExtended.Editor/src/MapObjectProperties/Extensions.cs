using MapsExt.MapObjects;
using MapsExt.Properties;
using System;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	public static class PropertyExtensions
	{
		public static T GetEditorMapObjectProperty<T>(this GameObject mapObject) where T : IProperty
		{
			return (T) mapObject.GetEditorMapObjectProperty(typeof(T));
		}

		public static IProperty GetEditorMapObjectProperty(this GameObject mapObject, Type propertyType)
		{
			var dataType = mapObject.GetComponent<MapObjectInstance>().DataType;

			if (MapsExtendedEditor.PropertyManager.GetSerializableMember(dataType, propertyType) == null)
			{
				return default;
			}

			return MapsExtendedEditor.PropertyManager.GetSerializer(propertyType)?.Serialize(mapObject);
		}

		public static void SetEditorMapObjectProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			var dataType = mapObject.GetComponent<MapObjectInstance>().DataType;

			if (MapsExtendedEditor.PropertyManager.GetSerializableMember(dataType, prop.GetType()) == null)
			{
				throw new ArgumentException($"{dataType.Name} does not have property {prop.GetType().Name}");
			}

			MapsExtendedEditor.PropertyManager.GetSerializer(prop.GetType()).Deserialize(prop, mapObject);
		}

		public static bool TrySetEditorMapObjectProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			try
			{
				mapObject.SetEditorMapObjectProperty(prop);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static T GetEditorMapObjectProperty<T>(this Component comp) where T : IProperty
		{
			return comp.gameObject.GetEditorMapObjectProperty<T>();
		}

		public static IProperty GetEditorMapObjectProperty(this Component comp, Type propertyType)
		{
			return comp.gameObject.GetEditorMapObjectProperty(propertyType);
		}

		public static void SetEditorMapObjectProperty<T>(this Component comp, T prop) where T : IProperty
		{
			comp.gameObject.SetEditorMapObjectProperty(prop);
		}

		public static bool TrySetEditorMapObjectProperty<T>(this Component comp, T prop) where T : IProperty
		{
			return comp.gameObject.TrySetEditorMapObjectProperty(prop);
		}
	}
}
