using UnityEngine;
using UnboundLib;
using MapsExt.Transformers;

namespace MapsExt.MapObjects
{
	public class Ball : SpatialMapObject { }

	[MapObjectSpec(typeof(Ball))]
	public static class BallSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Ball_Big");

		[MapObjectSerializer]
		public static void Serialize(GameObject instance, Ball target)
		{
			SpatialSerializer.Serialize(instance, target);
		}

		[MapObjectDeserializer]
		public static void Deserialize(Ball data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
			target.GetOrAddComponent<EllipseTransformer>();
		}
	}
}
