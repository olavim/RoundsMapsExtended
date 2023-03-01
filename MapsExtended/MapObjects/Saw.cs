using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SawData : SpatialMapObjectData, IMapObjectEllipse { }

	[MapObject]
	public class Saw : IMapObject<SawData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Saw");
	}
}
