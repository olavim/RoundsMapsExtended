using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObjectSerializer<T>
	{
		void Serialize(GameObject instance, T target);
		void Deserialize(T data, GameObject target);
	}
}
