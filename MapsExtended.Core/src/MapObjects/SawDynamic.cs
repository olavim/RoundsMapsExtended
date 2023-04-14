using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SawDynamicData : SpatialMapObjectData { }

	[MapObject(typeof(SawDynamicData))]
	public class SawDynamic : IMapObject
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Saw Dynamic");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
