using UnityEngine;
using UnboundLib;
using HarmonyLib;

namespace MapsExt.MapObjects
{
	public class SpatialMapObject : MapObject
	{
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one * 2;
		public Quaternion rotation = Quaternion.identity;

		public override MapObject Move(Vector3 v)
		{
			var copy = (SpatialMapObject) AccessTools.Constructor(this.GetType()).Invoke(new object[] { });
			copy.active = this.active;
			copy.position = this.position + v;
			copy.scale = this.scale;
			copy.rotation = this.rotation;
			return copy;
		}
	}

	/// <summary>
	///	Spatial map objects represent map objects that are described with position, scale and rotation.
	///	Typical spatial map objects are, for example, boxes and obstacles.
	/// </summary>
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
			/* SpatialMapObjectInstance doesn't add any functionality, but it offers a convenient way
			 * to find "spatial" map objects from scene.
			 */
			target.GetOrAddComponent<SpatialMapObjectInstance>();

			target.transform.position = data.position;
			target.transform.localScale = data.scale;
			target.transform.rotation = data.rotation;
		}
	}

	public class SpatialMapObjectInstance : MonoBehaviour { }
}
