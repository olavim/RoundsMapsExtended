using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxDestructible : SpatialMapObject { }

	[MapObjectSpec(typeof(BoxDestructible))]
	public static class BoxDestructibleSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_Destructible");

		[MapsExt.MapObjectSerializer]
		public static void Serialize(GameObject instance, BoxDestructible target)
		{
			SpatialSerializer.Serialize(instance, target);
		}

		[MapObjectDeserializer]
		public static void Deserialize(BoxDestructible data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
		}
	}
}
