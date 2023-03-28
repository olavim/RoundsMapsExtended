using MapsExt.MapObjects;
using UnityEngine;

#pragma warning disable CS0618

namespace MapsExt.Editor.Tests
{
	public class V0Box : SpatialMapObject { }

	[MapObjectSpec(typeof(V0Box))]
	public static class BoxSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box");

		[MapObjectSerializer]
		public static void Serialize(GameObject instance, V0Box target)
		{
			SpatialSerializer.Serialize(instance, target);
		}

		[MapObjectDeserializer]
		public static void Deserialize(V0Box data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
		}
	}
}

#pragma warning restore CS0618
