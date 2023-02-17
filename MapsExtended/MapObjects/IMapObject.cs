using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObject
	{
		GameObject Prefab { get; }
	}

	public interface IMapObject<T> : IMapObject where T : MapObjectData { }
}
