using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public class RotationProperty : ValueProperty<Quaternion>, ILinearProperty<RotationProperty>
	{
		private float _z;
		private float _w;

		public override Quaternion Value
		{
			get => new(0, 0, this._z, this._w);
			set { this._z = value.z; this._w = value.w; }
		}

		public RotationProperty() : base(Quaternion.identity) { }

		public RotationProperty(float angle) : base(Quaternion.Euler(0, 0, angle)) { }

		public RotationProperty(Quaternion value) : base(value) { }

		public RotationProperty Lerp(RotationProperty end, float t) => Quaternion.Lerp(this, end, t);
		public IProperty Lerp(IProperty end, float t) => this.Lerp((RotationProperty) end, t);

		public override bool Equals(ValueProperty<Quaternion> other) => base.Equals(other) || this.Value == other.Value;

		public static implicit operator Quaternion(RotationProperty prop) => prop.Value;
		public static implicit operator RotationProperty(Quaternion value) => new(value);
		public static implicit operator RotationProperty(float angle) => new(angle);

		public static RotationProperty operator *(RotationProperty a, RotationProperty b) => a.Value * b.Value;
		public static Vector3 operator *(RotationProperty a, Vector2 b) => a.Value * b;
		public static Vector3 operator *(RotationProperty a, Vector3 b) => a.Value * b;
	}

	[PropertySerializer(typeof(RotationProperty))]
	public class RotationPropertySerializer : PropertySerializer<RotationProperty>
	{
		public override RotationProperty Serialize(GameObject instance)
		{
			return instance.transform.rotation;
		}

		public override void Deserialize(RotationProperty property, GameObject target)
		{
			target.transform.rotation = property;
		}
	}
}
