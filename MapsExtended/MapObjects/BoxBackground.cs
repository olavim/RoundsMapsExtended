using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxBackground : SpatialMapObject { }

	[MapObjectBlueprint]
	public class BoxBackgroundBP : SpatialMapObjectBlueprint<BoxBackground>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_BG");
	}
}
