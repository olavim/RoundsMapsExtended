using UnboundLib;
using UnityEngine;

namespace MapsExt.Properties
{
	public class DamageableProperty : ValueProperty<bool>
	{
		private readonly bool _value;

		public override bool Value => this._value;

		public DamageableProperty() : this(true) { }

		public DamageableProperty(bool value)
		{
			this._value = value;
		}

		public static implicit operator bool(DamageableProperty prop) => prop.Value;
		public static implicit operator DamageableProperty(bool value) => new(value);
	}

	[PropertySerializer(typeof(DamageableProperty))]
	public class DamageablePropertySerializer : PropertySerializer<DamageableProperty>
	{
		public override DamageableProperty Serialize(GameObject instance)
		{
			var dmgInstance = instance.GetComponent<DamageableMapObjectInstance>();
			return dmgInstance.damageableByEnvironment;
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
