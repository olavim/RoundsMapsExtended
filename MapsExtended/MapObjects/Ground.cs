using UnityEngine;

namespace MapsExt.MapObjects
{
	public class Ground : SpatialMapObject { }

	[MapObjectSpec(typeof(Ground))]
	public static class GroundSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground");

		[MapsExt.MapObjectSerializer]
		public static void Serialize(GameObject instance, Ground target)
		{
			SpatialSerializer.Serialize(instance, target);
		}

		[MapObjectDeserializer]
		public static void Deserialize(Ground data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
		}
	}
}
