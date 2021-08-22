using MapsExt.Transformers;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class Saw : PhysicalMapObject { }

	[MapsExtendedMapObject(typeof(Saw))]
	public class SawSpecification : PhysicalMapObjectSpecification<Saw>
	{
		public override GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat");

		protected override void OnDeserialize(Saw data, GameObject target)
		{
			base.OnDeserialize(data, target);
			target.GetOrAddComponent<SawTransformer>();
		}
	}
}
