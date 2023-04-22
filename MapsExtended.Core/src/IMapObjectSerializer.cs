using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt
{
	public interface IMapObjectSerializer
	{
		void Deserialize(MapObjectData data, GameObject target);
	}
}
