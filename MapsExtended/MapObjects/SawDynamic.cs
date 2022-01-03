using MapsExt.Transformers;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class SawDynamic : SpatialMapObject { }

	[MapObjectBlueprint]
	public class SawDynamicBP : SpatialMapObjectBlueprint<SawDynamic>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw");

		public override void Deserialize(SawDynamic data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<SawTransformer>();
			target.GetOrAddComponent<EllipseTransformer>();
		}
	}
}
