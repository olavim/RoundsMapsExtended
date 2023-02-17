using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundData : SpatialMapObjectData { }

	[MapObject]
	public class Ground : IMapObject<GroundData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground");
	}
}
