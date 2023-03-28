using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxDestructibleData : SpatialMapObjectData
	{
		public DamageableProperty DamageableByEnvironment { get; set; } = new DamageableProperty();
	}

	[MapObject]
	public class BoxDestructible : IMapObject<BoxDestructibleData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Box Destructible");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
