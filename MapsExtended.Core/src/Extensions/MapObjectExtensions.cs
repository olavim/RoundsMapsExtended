using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt
{
	public static class MapObjectExtensions
	{
		public static MapObjectData ReadMapObject(this GameObject mapObject)
		{
			return MapObjectManager.Current.ReadMapObject(mapObject);
		}

		public static MapObjectData ReadMapObject(this Component comp)
		{
			return MapObjectManager.Current.ReadMapObject(comp.gameObject);
		}

		public static void WriteMapObject(this MapObjectData data, GameObject target)
		{
			MapObjectManager.Current.WriteMapObject(data, target);
		}
	}
}
