using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObject
	{
		GameObject Prefab { get; }
		void OnInstantiate(GameObject instance);
	}
}
