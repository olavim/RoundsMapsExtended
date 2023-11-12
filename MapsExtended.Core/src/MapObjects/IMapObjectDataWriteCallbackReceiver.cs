using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObjectDataWriteCallbackReceiver
	{
		void OnDataWrite(GameObject instance, MapObjectData data);
	}
}
