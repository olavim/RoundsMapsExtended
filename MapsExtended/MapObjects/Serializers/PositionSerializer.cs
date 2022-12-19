using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObjectPosition
	{
		Vector3 position { get; set; }
	}

	[MapObjectSerializer]
	public class PositionSerializer : IMapObjectSerializer<IMapObjectPosition>
	{
		public virtual void Serialize(GameObject instance, IMapObjectPosition target)
		{
			target.position = instance.transform.position;
		}

		public virtual void Deserialize(IMapObjectPosition data, GameObject target)
		{
			target.transform.position = data.position;
		}
	}
}
