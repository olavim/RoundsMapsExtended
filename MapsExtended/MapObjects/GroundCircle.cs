using UnityEngine;

namespace MapsExt.MapObjects
{
	public class GroundCircle : PhysicalMapObject { }

	[MapsExtendedMapObject(typeof(GroundCircle))]
	public class GroundCircleSpecification : PhysicalMapObjectSpecification<GroundCircle>
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground Circle");
	}
}
