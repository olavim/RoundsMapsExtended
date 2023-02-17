using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SawData : SpatialMapObjectData, IMapObjectEllipse, IMapObjectSaw { }

	[MapObject]
	public class Saw : IMapObject<SawData>
	{
		public GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw_Stat");
	}
}
