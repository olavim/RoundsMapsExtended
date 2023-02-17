using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IMapObjectPosition
	{
		Vector3 position { get; set; }
	}

	[MapObjectProperty]
	public class PositionProperty : IMapObjectProperty<IMapObjectPosition>
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
