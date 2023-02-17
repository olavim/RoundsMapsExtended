using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IMapObjectScale
	{
		Vector3 scale { get; set; }
	}

	[MapObjectProperty]
	public class ScaleProperty : IMapObjectProperty<IMapObjectScale>
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
