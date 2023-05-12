using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObjectSerializer
	{
		void WriteMapObject(MapObjectData data, GameObject target);
		MapObjectData ReadMapObject(MapObjectInstance mapObjectInstance);
	}
}
