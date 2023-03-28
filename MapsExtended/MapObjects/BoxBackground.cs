using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxBackgroundData : SpatialMapObjectData { }

	[MapObject]
	public class BoxBackground : IMapObject<BoxBackgroundData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Box Background");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
