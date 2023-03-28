using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SawData : SpatialMapObjectData { }

	[MapObject]
	public class Saw : IMapObject<SawData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Saw");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
