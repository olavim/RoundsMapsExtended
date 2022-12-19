using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObjectScale
	{
		Vector3 scale { get; set; }
	}

	[MapObjectSerializer]
	public class ScaleSerializer : IMapObjectSerializer<IMapObjectScale>
	{
		public virtual void Serialize(GameObject instance, IMapObjectScale target)
		{
			target.scale = instance.transform.localScale;
		}

		public virtual void Deserialize(IMapObjectScale data, GameObject target)
		{
			target.transform.localScale = data.scale;
		}
	}
}
