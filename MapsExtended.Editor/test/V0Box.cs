using MapsExt.Editor.MapObjects;
using MapsExt.MapObjects;
using UnityEngine;

#pragma warning disable CS0618

namespace MapsExt.Editor.Tests
{
	public class V0Box : SpatialMapObject { }

	[EditorMapObjectSpec(typeof(V0Box), "V0Box")]
	public static class BoxSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box");

		[EditorMapObjectSerializer]
		public static void Serialize(GameObject instance, V0Box target)
		{
			SpatialSerializer.Serialize(instance, target);
			EditorSpatialSerializer.Serialize(instance, target);
		}

		[EditorMapObjectDeserializer]
		public static void Deserialize(V0Box data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
			EditorSpatialSerializer.Deserialize(data, target);
		}
	}
}

#pragma warning restore CS0618
