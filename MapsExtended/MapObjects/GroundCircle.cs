using UnityEngine;
using UnboundLib;
using MapsExt.Transformers;

namespace MapsExt.MapObjects
{
	public class GroundCircle : SpatialMapObject { }

	[MapObjectBlueprint]
	public class GroundCircleBP : SpatialMapObjectBlueprint<GroundCircle>
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Ground Circle");

		public override void Deserialize(GroundCircle data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<EllipseTransformer>();
		}
	}
}
