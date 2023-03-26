using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public class RotationProperty : ValueProperty<Quaternion>, ILinearProperty<RotationProperty>
	{
		public RotationProperty() { }

		public RotationProperty(float angle) : base(Quaternion.Euler(0, 0, angle)) { }

		public RotationProperty(Quaternion value) : base(value) { }

		public RotationProperty Lerp(RotationProperty end, float t) => Quaternion.Lerp(this, end, t);
		public IMapObjectProperty Lerp(IMapObjectProperty end, float t) => this.Lerp((RotationProperty) end, t);

		public override bool Equals(ValueProperty<Quaternion> other) => base.Equals(other) || this.Value == other.Value;

		public static implicit operator Quaternion(RotationProperty prop) => prop.Value;
		public static implicit operator RotationProperty(Quaternion value) => new RotationProperty(value);
		public static implicit operator RotationProperty(float angle) => new RotationProperty(angle);

		public static RotationProperty operator *(RotationProperty a, RotationProperty b) => a.Value * b.Value;
		public static Vector3 operator *(RotationProperty a, Vector2 b) => a.Value * b;
		public static Vector3 operator *(RotationProperty a, Vector3 b) => a.Value * b;
	}

	[MapObjectPropertySerializer]
	public class RotationPropertySerializer : MapObjectPropertySerializer<RotationProperty>
	{
		public override void Serialize(GameObject instance, RotationProperty property)
		{
			property.Value = instance.transform.rotation;
		}

		public override void Deserialize(RotationProperty property, GameObject target)
		{
			target.transform.rotation = property;
		}
	}
}
