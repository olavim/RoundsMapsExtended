using UnboundLib;
using UnityEngine;

namespace MapsExt.Properties
{
	public class DamageableProperty : ValueProperty<bool>
	{
		[SerializeField] private readonly bool _value;

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
	public class DamageablePropertySerializer : IPropertyWriter<DamageableProperty>
	{
		public virtual void WriteProperty(DamageableProperty property, GameObject target)
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
