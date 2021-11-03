using MapsExt.Transformers;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class SawDynamic : SpatialMapObject { }

	[MapObjectSpec(typeof(SawDynamic))]
	public static class SawDynamicSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw");

		[MapsExt.MapObjectSerializer]
		public static void Serialize(GameObject instance, SawDynamic target)
		{
			SpatialSerializer.Serialize(instance, target);
		}

		[MapObjectDeserializer]
		public static void Deserialize(SawDynamic data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
			target.GetOrAddComponent<SawTransformer>();
			target.GetOrAddComponent<EllipseTransformer>();
		}
	}
}
