using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BallData : SpatialMapObjectData, IMapObjectEllipse { }

	[MapObject]
	public class Ball : IMapObject<BallData>
	{
		public GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Ball_Big");
	}
}
