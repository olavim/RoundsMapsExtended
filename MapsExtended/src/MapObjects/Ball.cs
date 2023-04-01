using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BallData : SpatialMapObjectData { }

	[MapObject(typeof(BallData))]
	public class Ball : IMapObject
	{
		public virtual GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Ball_Big");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
