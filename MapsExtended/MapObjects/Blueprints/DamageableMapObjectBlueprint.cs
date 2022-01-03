using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public abstract class DamageableMapObjectBlueprint<T> : SpatialMapObjectBlueprint<T> where T : DamageableMapObject
	{
		public override void Serialize(GameObject instance, T target)
		{
			base.Serialize(instance, target);
			var dmgInstance = instance.GetComponent<DamageableMapObjectInstance>();
			target.damageableByEnvironment = dmgInstance.damageableByEnvironment;
		}

		public override void Deserialize(T data, GameObject target)
		{
			base.Deserialize(data, target);
			var instance = target.GetOrAddComponent<DamageableMapObjectInstance>();
			instance.damageableByEnvironment = data.damageableByEnvironment;
		}
	}
}
