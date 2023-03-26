using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundCircleData : SpatialMapObjectData { }

	[MapObject]
	public class GroundCircle : IMapObject<GroundCircleData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground Circle");
	}
}
