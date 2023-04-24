using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundData : SpatialMapObjectData { }

	[MapObject(typeof(GroundData))]
	public class Ground : IMapObject
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
