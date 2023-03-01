using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SawDynamicData : SpatialMapObjectData, IMapObjectEllipse { }

	[MapObject]
	public class SawDynamic : IMapObject<SawDynamicData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Saw Dynamic");
	}
}
