using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public abstract class PropertySerializer<T> : IPropertySerializer<T> where T : IProperty
	{
		public void Deserialize(IProperty property, GameObject target)
		{
			this.Deserialize((T) property, target);
		}

		void IPropertySerializer.Serialize(GameObject instance, IProperty property)
		{
			this.Serialize(instance, (T) property);
		}

		public abstract void Deserialize(T property, GameObject target);
		public abstract void Serialize(GameObject instance, T property);
	}
}
