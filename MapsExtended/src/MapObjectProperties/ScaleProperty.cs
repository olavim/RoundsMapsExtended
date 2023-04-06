using UnityEngine;

namespace MapsExt.Properties
{
	public class ScaleProperty : ValueProperty<Vector2>, ILinearProperty<ScaleProperty>
	{
		private float _x;
		private float _y;

		public override Vector2 Value
		{
			get => new(this._x, this._y);
			set { this._x = value.x; this._y = value.y; }
		}

		public ScaleProperty() : base(new Vector2(2, 2)) { }

		public ScaleProperty(Vector2 value) : base(value) { }

		public ScaleProperty(float x, float y) : base(new Vector2(x, y)) { }

		public ScaleProperty Lerp(ScaleProperty end, float t) => Vector2.Lerp(this, end, t);
		public IProperty Lerp(IProperty end, float t) => this.Lerp((ScaleProperty) end, t);

		public static implicit operator Vector2(ScaleProperty prop) => prop.Value;
		public static implicit operator Vector3(ScaleProperty prop) => prop.Value;
		public static implicit operator ScaleProperty(Vector2 value) => new(value);
		public static implicit operator ScaleProperty(Vector3 value) => new(value);

		public static ScaleProperty operator -(ScaleProperty a, ScaleProperty b) => a.Value - b.Value;
		public static ScaleProperty operator +(ScaleProperty a, ScaleProperty b) => a.Value + b.Value;
	}

	[PropertySerializer(typeof(ScaleProperty))]
	public class ScalePropertySerializer : PropertySerializer<ScaleProperty>
	{
		public override ScaleProperty Serialize(GameObject instance)
		{
			return instance.transform.localScale;
		}

		public override void Deserialize(ScaleProperty property, GameObject target)
		{
			target.transform.localScale = property;
		}
	}
}
