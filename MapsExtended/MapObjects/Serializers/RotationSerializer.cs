using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObjectRotation
	{
		Quaternion rotation { get; set; }
	}

	[MapObjectSerializer]
	public class RotationSerializer : IMapObjectSerializer<IMapObjectRotation>
	{
		public virtual void Serialize(GameObject instance, IMapObjectRotation target)
		{
			target.rotation = instance.transform.rotation;
		}

		public virtual void Deserialize(IMapObjectRotation data, GameObject target)
		{
			target.transform.rotation = data.rotation;
		}
	}
}
