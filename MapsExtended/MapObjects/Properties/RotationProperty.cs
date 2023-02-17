using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IMapObjectRotation
	{
		Quaternion rotation { get; set; }
	}

	[MapObjectProperty]
	public class RotationProperty : IMapObjectProperty<IMapObjectRotation>
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
