using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BallData : SpatialMapObjectData { }

	[MapObject(typeof(BallData))]
	public class Ball : IMapObject
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ball");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
