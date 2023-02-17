using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SawDynamicData : SpatialMapObjectData, IMapObjectEllipse, IMapObjectSaw { }

	[MapObject]
	public class SawDynamic : IMapObject<SawDynamicData>
	{
		public GameObject Prefab => Resources.Load<GameObject>("4 Map Objects/MapObject_Saw");
	}
}
