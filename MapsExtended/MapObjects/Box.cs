using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxData : SpatialMapObjectData { }

	[MapObject]
	public class Box : IMapObject<BoxData>
	{
		public GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box");
	}
}
