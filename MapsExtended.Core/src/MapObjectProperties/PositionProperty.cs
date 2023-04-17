using UnityEngine;

namespace MapsExt.Properties
{
	public class PositionProperty : ValueProperty<Vector2>, ILinearProperty<PositionProperty>
	{
		private readonly float _x;
		private readonly float _y;

		public override Vector2 Value => new(this._x, this._y);

		public PositionProperty() { }

		public PositionProperty(Vector2 value) : this(value.x, value.y) { }

		public PositionProperty(float x, float y)
		{
			this._x = x;
			this._y = y;
		}

		public PositionProperty Lerp(PositionProperty end, float t) => Vector2.Lerp(this, end, t);
		public IProperty Lerp(IProperty end, float t) => this.Lerp((PositionProperty) end, t);

		public static implicit operator Vector2(PositionProperty prop) => prop.Value;
		public static implicit operator Vector3(PositionProperty prop) => prop.Value;
		public static implicit operator PositionProperty(Vector2 value) => new(value);
		public static implicit operator PositionProperty(Vector3 value) => new(value);

		public static PositionProperty operator *(RotationProperty a, PositionProperty b) => a.Value * b.Value;
		public static PositionProperty operator +(PositionProperty a, PositionProperty b) => a.Value + b.Value;
		public static PositionProperty operator -(PositionProperty a, PositionProperty b) => a.Value - b.Value;
	}

	[PropertySerializer(typeof(PositionProperty))]
	public class PositionPropertySerializer : PropertySerializer<PositionProperty>
	{
		public override void Deserialize(PositionProperty property, GameObject target)
		{
			target.transform.position = property;
		}

		public override PositionProperty Serialize(GameObject instance)
		{
			return instance.transform.position;
		}
	}
}
