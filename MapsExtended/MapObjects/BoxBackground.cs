using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxBackgroundData : SpatialMapObjectData { }

	[MapObject]
	public class BoxBackground : IMapObject<BoxBackgroundData>
	{
		public GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_BG");
	}
}
