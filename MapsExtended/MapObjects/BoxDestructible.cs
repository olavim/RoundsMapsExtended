using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxDestructible : DamageableMapObject { }

	[MapObjectSpec(typeof(BoxDestructible))]
	public static class BoxDestructibleSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_Destructible");

		[MapsExt.MapObjectSerializer]
		public static void Serialize(GameObject instance, BoxDestructible target)
		{
			DamageableSerializer.Serialize(instance, target);
		}

		[MapObjectDeserializer]
		public static void Deserialize(BoxDestructible data, GameObject target)
		{
			DamageableSerializer.Deserialize(data, target);
		}
	}
}
