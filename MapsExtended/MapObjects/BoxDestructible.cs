using UnityEngine;

namespace MapsExt.MapObjects
{
	public class BoxDestructible : DamageableMapObject { }

	[MapObjectBlueprint]
	public class BoxDestructibleBP : DamageableMapObjectBlueprint<BoxDestructible>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Box_Destructible");
	}
}
