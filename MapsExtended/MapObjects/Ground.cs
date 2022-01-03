using UnityEngine;

namespace MapsExt.MapObjects
{
	public class Ground : SpatialMapObject { }

	[MapObjectBlueprint]
	public class GroundBP : SpatialMapObjectBlueprint<Ground>
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground");
	}
}
