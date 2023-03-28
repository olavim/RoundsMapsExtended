using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundData : SpatialMapObjectData { }

	[MapObject]
	public class Ground : IMapObject<GroundData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
