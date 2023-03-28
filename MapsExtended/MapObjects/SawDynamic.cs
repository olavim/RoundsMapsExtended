using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SawDynamicData : SpatialMapObjectData { }

	[MapObject]
	public class SawDynamic : IMapObject<SawDynamicData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Saw Dynamic");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
