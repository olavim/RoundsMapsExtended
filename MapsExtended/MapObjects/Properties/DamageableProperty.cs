using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IMapObjectDamageable
	{
		bool damageableByEnvironment { get; set; }
	}

	[MapObjectProperty]
	public class DamageableProperty : IMapObjectProperty<IMapObjectDamageable>
	{
		public virtual void Serialize(GameObject instance, IMapObjectDamageable target)
		{
			var dmgInstance = instance.GetComponent<DamageableMapObjectInstance>();
			target.damageableByEnvironment = dmgInstance.damageableByEnvironment;
		}

		public virtual void Deserialize(IMapObjectDamageable data, GameObject target)
		{
			var instance = target.GetOrAddComponent<DamageableMapObjectInstance>();
			instance.damageableByEnvironment = data.damageableByEnvironment;
		}
	}

	public class DamageableMapObjectInstance : MonoBehaviour
	{
		public bool damageableByEnvironment;
	}
}
