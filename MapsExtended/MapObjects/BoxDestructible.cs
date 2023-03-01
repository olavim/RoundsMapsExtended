using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxDestructibleData : SpatialMapObjectData, IMapObjectDamageable
	{
		public bool damageableByEnvironment { get; set; } = false;
	}

	[MapObject]
	public class BoxDestructible : IMapObject<BoxDestructibleData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Box Destructible");
	}
}
