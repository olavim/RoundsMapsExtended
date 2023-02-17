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
		public GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_Destructible");
	}
}
