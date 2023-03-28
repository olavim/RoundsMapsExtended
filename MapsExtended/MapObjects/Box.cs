using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxData : SpatialMapObjectData { }

	[MapObject]
	public class Box : IMapObject<BoxData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Box");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
