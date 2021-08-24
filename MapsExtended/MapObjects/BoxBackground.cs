using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxBackground : SpatialMapObject { }
	
	[MapObjectSpec(typeof(BoxBackground))]
	public static class BoxBackgroundSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_BG");
		
		[MapsExt.MapObjectSerializer]
		public static void Serialize(GameObject instance, BoxBackground target)
		{
			SpatialSerializer.Serialize(instance, target);
		}
		
		[MapObjectDeserializer]
		public static void Deserialize(BoxBackground data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
		}
	}
}
