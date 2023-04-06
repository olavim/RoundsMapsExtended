using UnityEngine;

namespace MapsExt.Properties
{
	public interface IPropertySerializer
	{
		IProperty Serialize(GameObject instance);
		void Deserialize(IProperty property, GameObject target);
	}

	public interface IPropertySerializer<T> : IPropertySerializer where T : IProperty
	{
		new T Serialize(GameObject instance);
		void Deserialize(T property, GameObject target);
	}
}
