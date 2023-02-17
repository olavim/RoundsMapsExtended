using UnboundLib;
using UnityEngine;
using MapsExt.Transformers;

namespace MapsExt.MapObjects.Properties
{
	public interface IMapObjectEllipse { }

	[MapObjectProperty]
	public class EllipseProperty : IMapObjectProperty<IMapObjectEllipse>
	{
		public virtual void Serialize(GameObject instance, IMapObjectEllipse target) { }

		public virtual void Deserialize(IMapObjectEllipse data, GameObject target)
		{
			target.GetOrAddComponent<EllipseTransformer>();
		}
	}
}
