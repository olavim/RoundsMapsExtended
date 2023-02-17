using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundCircleData : SpatialMapObjectData, IMapObjectEllipse { }

	[MapObject]
	public class GroundCircle : IMapObject<GroundCircleData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground Circle");
	}
}
