using UnityEngine;

namespace MapsExt.MapObjects
{
	public class Box : SpatialMapObject { }

	[MapObjectSpec(typeof(Box))]
	public static class BoxSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box");

		[MapsExt.MapObjectSerializer]
		public static void Serialize(GameObject instance, Box target)
		{
			SpatialSerializer.Serialize(instance, target);
		}

		[MapObjectDeserializer]
		public static void Deserialize(Box data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
		}
	}
}
