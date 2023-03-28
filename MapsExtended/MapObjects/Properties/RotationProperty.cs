using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public class RotationProperty : ValueProperty<Quaternion>, ILinearProperty<RotationProperty>
	{
		public RotationProperty() : base(Quaternion.identity) { }

		public RotationProperty(float angle) : base(Quaternion.Euler(0, 0, angle)) { }

		public RotationProperty(Quaternion value) : base(value) { }

		public RotationProperty Lerp(RotationProperty end, float t) => Quaternion.Lerp(this, end, t);
		public IProperty Lerp(IProperty end, float t) => this.Lerp((RotationProperty) end, t);

		public override bool Equals(ValueProperty<Quaternion> other) => base.Equals(other) || this.value == other.value;

		public static implicit operator Quaternion(RotationProperty prop) => prop.value;
		public static implicit operator RotationProperty(Quaternion value) => new RotationProperty(value);
		public static implicit operator RotationProperty(float angle) => new RotationProperty(angle);

		public static RotationProperty operator *(RotationProperty a, RotationProperty b) => a.value * b.value;
		public static Vector3 operator *(RotationProperty a, Vector2 b) => a.value * b;
		public static Vector3 operator *(RotationProperty a, Vector3 b) => a.value * b;
	}

	[PropertySerializer]
	public class RotationPropertySerializer : PropertySerializer<RotationProperty>
	{
		public override void Serialize(GameObject instance, RotationProperty property)
		{
			property.value = instance.transform.rotation;
		}

		public override void Deserialize(RotationProperty property, GameObject target)
		{
			target.transform.rotation = property;
		}
	}
}
