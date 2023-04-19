using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	public static class MapObjectExtensions
	{
		public static MapObjectData SerializeEditorMapObject(this GameObject mapObject)
		{
			return MapsExtendedEditor.MapObjectManager.Serialize(mapObject);
		}

		public static MapObjectData SerializeEditorMapObject(this Component comp)
		{
			return MapsExtendedEditor.MapObjectManager.Serialize(comp.gameObject);
		}

		public static void DeserializeEditorMapObject(this MapObjectData data, GameObject target)
		{
			MapsExtendedEditor.MapObjectManager.Deserialize(data, target);
		}
	}
}
