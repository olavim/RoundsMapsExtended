using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IMapObjectProperty<T>
	{
		void Serialize(GameObject instance, T target);
		void Deserialize(T data, GameObject target);
	}
}
