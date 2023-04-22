using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	public static class MapObjectExtensions
	{
		public static MapObjectData SerializeMapObject(this GameObject mapObject)
		{
			return MapsExtendedEditor.MapObjectManager.Serialize(mapObject);
		}

		public static MapObjectData SerializeMapObject(this Component comp)
		{
			return MapsExtendedEditor.MapObjectManager.Serialize(comp.gameObject);
		}

		public static void DeserializeMapObject(this MapObjectData data, GameObject target)
		{
			MapsExtendedEditor.MapObjectManager.Deserialize(data, target);
		}
	}
}
