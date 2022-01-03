using MapsExt.Transformers;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class Saw : SpatialMapObject { }

	[MapObjectBlueprint]
	public class SawBP : SpatialMapObjectBlueprint<Saw>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat");

		public override void Deserialize(Saw data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<SawTransformer>();
			target.GetOrAddComponent<EllipseTransformer>();
		}
	}
}
