using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxData : SpatialMapObjectData { }

	[MapObject(typeof(BoxData))]
	public class Box : IMapObject
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Box");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
