using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	public static class PropertyExtensions
	{
		public static T GetEditorMapObjectProperty<T>(this GameObject mapObject) where T : IProperty
		{
			var data = MapsExtendedEditor.instance.MapObjectManager.Serialize(mapObject);
			return MapsExtendedEditor.instance.PropertyManager.GetProperty<T>(data);
		}

		public static T GetEditorMapObjectProperty<T>(this Component comp) where T : IProperty
		{
			return comp.gameObject.GetEditorMapObjectProperty<T>();
		}

		public static void SetEditorMapObjectProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			var data = MapsExtendedEditor.instance.MapObjectManager.Serialize(mapObject);
			MapsExtendedEditor.instance.PropertyManager.SetProperty(data, prop);
			MapsExtendedEditor.instance.MapObjectManager.Deserialize(data, mapObject);
		}

		public static void SetEditorMapObjectProperty<T>(this Component comp, T prop) where T : IProperty
		{
			comp.gameObject.SetEditorMapObjectProperty(prop);
		}

		public static bool TrySetEditorMapObjectProperty<T>(this GameObject mapObject, T prop) where T : IProperty
		{
			var data = MapsExtendedEditor.instance.MapObjectManager.Serialize(mapObject);
			bool result = MapsExtendedEditor.instance.PropertyManager.TrySetProperty(data, prop);
			MapsExtendedEditor.instance.MapObjectManager.Deserialize(data, mapObject);
			return result;
		}

		public static bool TrySetEditorMapObjectProperty<T>(this Component comp, T prop) where T : IProperty
		{
			return comp.gameObject.TrySetEditorMapObjectProperty(prop);
		}
	}
}
