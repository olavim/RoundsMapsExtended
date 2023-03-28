using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IPropertySerializer
	{
		void Serialize(GameObject instance, IProperty property);
		void Deserialize(IProperty property, GameObject target);
	}

	public interface IPropertySerializer<T> : IPropertySerializer where T : IProperty
	{
		void Serialize(GameObject instance, T property);
		void Deserialize(T property, GameObject target);
	}
}
