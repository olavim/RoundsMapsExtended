using UnityEngine;
using HarmonyLib;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public abstract class PhysicalMapObject : MapObject
	{
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one * 2;
		public Quaternion rotation = Quaternion.identity;
	}

	/// <summary>
	///	 Physical map objects represent map objects that are described with position, scale and rotation.
	///	 Typical physical map objects are, for example, boxes and obstacles. Here "physical" does not mean that
	///	 a map object is or isn't a dynamic (physics-driven) object; physical objects can be static or dynamic.
	///	 Physical map objects are also not necessarily collidable.
	/// </summary>
	public abstract class PhysicalMapObjectSpecification<T> : MapObjectSpecification<T>
		where T : PhysicalMapObject
	{
		protected override void OnDeserialize(T data, GameObject target)
		{
			/* PhysicalMapObjectInstance doesn't add any functionality, but it offers a convenient way
			 * to find "physical" map objects from scene.
			 */
			target.GetOrAddComponent<PhysicalMapObjectInstance>();

			target.transform.position = data.position;
			target.transform.localScale = data.scale;
			target.transform.rotation = data.rotation;
		}

		protected override T OnSerialize(GameObject instance)
		{
			var data = AccessTools.CreateInstance<T>();
			data.position = instance.transform.position;
			data.scale = instance.transform.localScale;
			data.rotation = instance.transform.rotation;
			return data;
		}
	}

	public class PhysicalMapObjectInstance : MonoBehaviour { }
}
