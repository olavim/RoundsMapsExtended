using UnityEngine;

namespace MapsExt.MapObjects
{
	public class Box : SpatialMapObject { }

	[MapObjectBlueprint]
	public class BoxBP : SpatialMapObjectBlueprint<Box>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box");
	}
}
