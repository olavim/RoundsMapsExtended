using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxDestructibleData : SpatialMapObjectData
	{
		private bool _damageable;

		public DamageableProperty DamageableByEnvironment { get => this._damageable; set => this._damageable = value; }

		public BoxDestructibleData()
		{
			this.DamageableByEnvironment = new DamageableProperty();
		}
	}

	[MapObject(typeof(BoxDestructibleData))]
	public class BoxDestructible : IMapObject
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Box Destructible");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
