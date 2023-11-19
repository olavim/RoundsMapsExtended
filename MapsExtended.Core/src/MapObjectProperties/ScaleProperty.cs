using UnityEngine;

namespace MapsExt.Properties
{
	public class ScaleProperty : ValueProperty<Vector2>, ILinearProperty<ScaleProperty>
	{
		[SerializeField] private readonly float _x;
		[SerializeField] private readonly float _y;

		public override Vector2 Value => new(this._x, this._y);

		public ScaleProperty() : this(2, 2) { }

		public ScaleProperty(Vector2 value) : this(value.x, value.y) { }

		public ScaleProperty(float x, float y)
		{
			this._x = x;
			this._y = y;
		}

		public ScaleProperty Lerp(ScaleProperty end, float t) => Vector2.Lerp(this, end, t);
		public IProperty Lerp(IProperty end, float t) => this.Lerp((ScaleProperty) end, t);

		public static implicit operator Vector2(ScaleProperty prop) => prop.Value;
		public static implicit operator Vector3(ScaleProperty prop) => prop.Value;
		public static implicit operator ScaleProperty(Vector2 value) => new(value);
		public static implicit operator ScaleProperty(Vector3 value) => new(value);

		public static ScaleProperty operator +(ScaleProperty a, ScaleProperty b) => new(a.Value + b.Value);
		public static ScaleProperty operator -(ScaleProperty a, ScaleProperty b) => new(a.Value - b.Value);
	}

	[PropertySerializer(typeof(ScaleProperty))]
	public class ScalePropertySerializer : IPropertyWriter<ScaleProperty>
	{
		public virtual void WriteProperty(ScaleProperty property, GameObject target)
		{
			target.transform.localScale = property;
		}
	}
}
