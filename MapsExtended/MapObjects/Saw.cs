using MapsExt.Transformers;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class Saw : SpatialMapObject { }

	[MapObjectSpec(typeof(Saw))]
	public static class SawSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat");

		[MapsExt.MapObjectSerializer]
		public static void Serialize(GameObject instance, Saw target)
		{
			SpatialSerializer.Serialize(instance, target);
		}

		[MapObjectDeserializer]
		public static void Deserialize(Saw data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
			target.GetOrAddComponent<SawTransformer>();
		}
	}
}
