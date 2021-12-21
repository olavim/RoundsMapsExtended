using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class DamageableMapObject : SpatialMapObject
	{
		public bool damageableByEnvironment = false;
	}

	/// <summary>
	///	Spatial map objects represent map objects that are described with position, scale and rotation.
	///	Typical spatial map objects are, for example, boxes and obstacles.
	/// </summary>
	public static class DamageableSerializer
	{
		public static void Serialize(GameObject instance, DamageableMapObject target)
		{
			SpatialSerializer.Serialize(instance, target);
			var dmgInstance = instance.GetComponent<DamageableMapObjectInstance>();
			target.damageableByEnvironment = dmgInstance.damageableByEnvironment;
		}

		public static void Deserialize(DamageableMapObject data, GameObject target)
		{
			SpatialSerializer.Deserialize(data, target);
			var instance = target.GetOrAddComponent<DamageableMapObjectInstance>();
			instance.damageableByEnvironment = data.damageableByEnvironment;
		}
	}

	public class DamageableMapObjectInstance : MonoBehaviour
	{
		public bool damageableByEnvironment;
	}
}
