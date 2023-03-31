using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SawData : SpatialMapObjectData { }

	[MapObject(typeof(SawData))]
	public class Saw : IMapObject
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Saw");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
