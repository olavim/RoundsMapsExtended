using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt
{
	public interface IMapObjectSerializer
	{
		MapObjectData Serialize(MapObjectInstance mapObjectInstance);
		void Deserialize(MapObjectData data, GameObject target);
	}
}
