using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public class DamageableProperty : ValueProperty<bool>
	{
		public DamageableProperty() : this(true) { }

		public DamageableProperty(bool value) : base(value) { }

		public static implicit operator bool(DamageableProperty prop) => prop.Value;
		public static implicit operator DamageableProperty(bool value) => new DamageableProperty(value);
	}

	[PropertySerializer]
	public class DamageablePropertySerializer : PropertySerializer<DamageableProperty>
	{
		public override void Serialize(GameObject instance, DamageableProperty property)
		{
			var dmgInstance = instance.GetComponent<DamageableMapObjectInstance>();
			property.Value = dmgInstance.damageableByEnvironment;
		}

		public override void Deserialize(DamageableProperty property, GameObject target)
		{
			var instance = target.GetOrAddComponent<DamageableMapObjectInstance>();
			instance.damageableByEnvironment = property;
		}
	}

	public class DamageableMapObjectInstance : MonoBehaviour
	{
		public bool damageableByEnvironment;
	}
}
