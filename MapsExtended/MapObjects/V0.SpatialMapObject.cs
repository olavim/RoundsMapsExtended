using System;
using UnityEngine;

namespace MapsExt.MapObjects
{
	[Obsolete("Deprecated")]
	public class SpatialMapObject : MapObject
	{
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one * 2;
		public Quaternion rotation = Quaternion.identity;
	}

	[Obsolete("Deprecated")]
	public static class SpatialSerializer
	{
		public static void Serialize(GameObject instance, SpatialMapObject target)
		{
			target.position = instance.transform.position;
			target.scale = instance.transform.localScale;
			target.rotation = instance.transform.rotation;
		}

		public static void Deserialize(SpatialMapObject data, GameObject target)
		{
			target.transform.position = data.position;
			target.transform.localScale = data.scale;
			target.transform.rotation = data.rotation;
		}
	}
}