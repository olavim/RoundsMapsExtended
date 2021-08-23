using MapsExt.Transformers;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class SawDynamic : PhysicalMapObject { }

	[MapsExtendedMapObject(typeof(SawDynamic))]
	public class SawDynamicSpecification : PhysicalMapObjectSpecification<SawDynamic>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw");

		protected override void Deserialize(SawDynamic data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<SawTransformer>();
		}
	}
}
