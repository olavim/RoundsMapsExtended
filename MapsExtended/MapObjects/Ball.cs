using UnityEngine;
using UnboundLib;
using MapsExt.Transformers;

namespace MapsExt.MapObjects
{
	public class Ball : SpatialMapObject { }

	[MapObjectBlueprint]
	public class BallBP : SpatialMapObjectBlueprint<Ball>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/Ball_Big");

		public override void Deserialize(Ball data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<EllipseTransformer>();
		}
	}
}
