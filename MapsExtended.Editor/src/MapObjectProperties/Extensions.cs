using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	public static class PropertyExtensions
	{
		public static T GetEditorMapObjectProperty<T>(this GameObject mapObject) where T : IProperty
		{
			return (T) MapsExtendedEditor.instance.PropertyManager.GetSerializer<T>().Serialize(mapObject);
		}

		public static T GetEditorMapObjectProperty<T>(this Component comp) where T : IProperty
		{
			return comp.gameObject.GetEditorMapObjectProperty<T>();
		}

		public static void SetEditorMapObjectProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			MapsExtendedEditor.instance.PropertyManager.GetSerializer<T>().Deserialize(prop, mapObject);
		}

		public static void SetEditorMapObjectProperty<T>(this Component comp, T prop) where T : IProperty
		{
			comp.gameObject.SetEditorMapObjectProperty(prop);
		}

		public static bool TrySetEditorMapObjectProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			var serializer = MapsExtendedEditor.instance.PropertyManager.GetSerializer<T>();
			if (serializer == null)
			{
				return false;
			}
			serializer.Deserialize(prop, mapObject);
			return true;
		}

		public static bool TrySetEditorMapObjectProperty<T>(this Component comp, T prop) where T : IProperty
		{
			return comp.gameObject.TrySetEditorMapObjectProperty(prop);
		}
	}
}
