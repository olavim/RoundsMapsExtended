using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public class ScaleProperty : ValueProperty<Vector2>, ILinearProperty<ScaleProperty>
	{
		public ScaleProperty() : base(new Vector2(2, 2)) { }

		public ScaleProperty(Vector2 value) : base(value) { }

		public ScaleProperty(float x, float y) : base(new Vector2(x, y)) { }

		public ScaleProperty Lerp(ScaleProperty end, float t) => Vector2.Lerp(this, end, t);
		public IMapObjectProperty Lerp(IMapObjectProperty end, float t) => this.Lerp((ScaleProperty) end, t);

		public static implicit operator Vector2(ScaleProperty prop) => prop.Value;
		public static implicit operator Vector3(ScaleProperty prop) => prop.Value;
		public static implicit operator ScaleProperty(Vector2 value) => new ScaleProperty(value);
		public static implicit operator ScaleProperty(Vector3 value) => new ScaleProperty(value);

		public static ScaleProperty operator -(ScaleProperty a, ScaleProperty b) => a.Value - b.Value;
		public static ScaleProperty operator +(ScaleProperty a, ScaleProperty b) => a.Value + b.Value;
	}

	[MapObjectPropertySerializer]
	public class ScalePropertySerializer : MapObjectPropertySerializer<ScaleProperty>
	{
		public override void Serialize(GameObject instance, ScaleProperty property)
		{
			property.Value = instance.transform.localScale;
		}

		public override void Deserialize(ScaleProperty property, GameObject target)
		{
			target.transform.localScale = property;
		}
	}
}