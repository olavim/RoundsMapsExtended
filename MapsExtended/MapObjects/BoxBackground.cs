using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxBackgroundData : SpatialMapObjectData { }

	[MapObject]
	public class BoxBackground : IMapObject<BoxBackgroundData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Box Background");
	}
}
