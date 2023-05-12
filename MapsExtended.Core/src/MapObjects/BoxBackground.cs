using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxBackgroundData : SpatialMapObjectData { }

	[MapObject(typeof(BoxBackgroundData))]
	public class BoxBackground : IMapObject
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Box Background");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
