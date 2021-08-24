using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundCircle : SpatialMapObject { }
	
	[MapObjectSpec(typeof(GroundCircle))]
	public static class GroundCircleSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground Circle");
		
		[MapsExt.MapObjectSerializer]
		public static void Serialize(GameObject instance, GroundCircle target)
		{
			SpatialSerializer.Serialize(instance, target);
		}
		
		[MapObjectDeserializer]
		public static void Deserialize(GroundCircle data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
		}
	}
}
