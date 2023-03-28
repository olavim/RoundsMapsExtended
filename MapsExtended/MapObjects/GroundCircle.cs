using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundCircleData : SpatialMapObjectData { }

	[MapObject]
	public class GroundCircle : IMapObject<GroundCircleData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground Circle");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
