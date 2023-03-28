using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObject
	{
		GameObject Prefab { get; }
		void OnInstantiate(GameObject instance);
	}

	public interface IMapObject<T> : IMapObject where T : MapObjectData { }
}
