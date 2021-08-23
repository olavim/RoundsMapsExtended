using UnityEngine;
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
		protected override void Deserialize(T data, GameObject target)
		{
			/* PhysicalMapObjectInstance doesn't add any functionality, but it offers a convenient way
			 * to find "physical" map objects from scene.
			 */
			target.GetOrAddComponent<PhysicalMapObjectInstance>();

			target.transform.position = data.position;
			target.transform.localScale = data.scale;
			target.transform.rotation = data.rotation;
		}

		protected override void Serialize(GameObject instance, T target)
		{
			target.position = instance.transform.position;
			target.scale = instance.transform.localScale;
			target.rotation = instance.transform.rotation;
		}
	}

	public class PhysicalMapObjectInstance : MonoBehaviour { }
}
