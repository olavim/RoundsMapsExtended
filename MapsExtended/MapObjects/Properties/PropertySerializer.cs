using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public abstract class PropertySerializer<T> : IPropertySerializer<T> where T : IProperty
	{
		public void Deserialize(IProperty property, GameObject target) => this.Deserialize((T) property, target);
		IProperty IPropertySerializer.Serialize(GameObject instance) => this.Serialize(instance);

		public abstract void Deserialize(T property, GameObject target);
		public abstract T Serialize(GameObject instance);
	}
}
