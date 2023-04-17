using UnboundLib;
using UnityEngine;

namespace MapsExt.Properties
{
	public class RotationProperty : ValueProperty<float>, ILinearProperty<RotationProperty>
	{
		private readonly float _angle;

		public override float Value => this._angle;

		public RotationProperty() { }

		public RotationProperty(float angle)
		{
			this._angle = angle;
		}

		public override bool Equals(ValueProperty<float> other) => base.Equals(other) || this.Value == other.Value;

		public RotationProperty Lerp(RotationProperty end, float t) => new(Mathf.LerpAngle(this.Value, end.Value, t));
		public IProperty Lerp(IProperty end, float t) => this.Lerp((RotationProperty) end, t);

		public static explicit operator Quaternion(RotationProperty prop) => Quaternion.Euler(0, 0, prop.Value % 360);
		public static implicit operator RotationProperty(float angle) => new(angle);
		public static implicit operator float(RotationProperty prop) => prop.Value;

		public static RotationProperty operator +(RotationProperty a, RotationProperty b) => a.Value + b.Value;
		public static RotationProperty operator -(RotationProperty a, RotationProperty b) => a.Value - b.Value;
		public static Vector3 operator *(RotationProperty a, Vector2 b) => (Quaternion) a * b;
		public static Vector3 operator *(RotationProperty a, Vector3 b) => (Quaternion) a * b;
	}

	[PropertySerializer(typeof(RotationProperty))]
	public class RotationPropertySerializer : PropertySerializer<RotationProperty>
	{
		public override RotationProperty Serialize(GameObject instance)
		{
			return instance.GetComponent<RotationPropertyInstance>().Rotation;
		}

		public override void Deserialize(RotationProperty property, GameObject target)
		{
			target.GetOrAddComponent<RotationPropertyInstance>().Rotation = property;
			target.transform.rotation = (Quaternion) property;
		}
	}

	public class RotationPropertyInstance : MonoBehaviour
	{
		public RotationProperty Rotation { get; set; }
	}
}
