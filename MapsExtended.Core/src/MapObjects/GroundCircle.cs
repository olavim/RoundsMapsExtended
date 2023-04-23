using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundCircleData : SpatialMapObjectData { }

	[MapObject(typeof(GroundCircleData))]
	public class GroundCircle : IMapObject
	{
		public virtual GameObject Prefab => NetworkedMapObjectManager.LoadCustomAsset<GameObject>("Ground Circle");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
