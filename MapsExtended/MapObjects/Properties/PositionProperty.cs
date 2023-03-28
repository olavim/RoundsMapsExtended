using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public class PositionProperty : ValueProperty<Vector2>, ILinearProperty<PositionProperty>
	{
		public PositionProperty() { }

		public PositionProperty(Vector2 value) : base(value) { }

		public PositionProperty(float x, float y) : base(new Vector2(x, y)) { }

		public PositionProperty Lerp(PositionProperty end, float t) => Vector2.Lerp(this, end, t);
		public IProperty Lerp(IProperty end, float t) => this.Lerp((PositionProperty) end, t);

		public static implicit operator Vector2(PositionProperty prop) => prop.value;
		public static implicit operator Vector3(PositionProperty prop) => prop.value;
		public static implicit operator PositionProperty(Vector2 value) => new PositionProperty(value);
		public static implicit operator PositionProperty(Vector3 value) => new PositionProperty(value);
	}

	[PropertySerializer]
	public class PositionPropertySerializer : PropertySerializer<PositionProperty>
	{
		public override void Deserialize(PositionProperty property, GameObject target)
		{
			target.transform.position = property;
		}

		public override void Serialize(GameObject instance, PositionProperty property)
		{
			property.value = instance.transform.position;
		}
	}
}
