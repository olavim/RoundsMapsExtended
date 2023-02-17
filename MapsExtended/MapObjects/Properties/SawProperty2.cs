using UnboundLib;
using UnityEngine;
using MapsExt.Transformers;

namespace MapsExt.MapObjects.Properties
{
	public interface IMapObjectSaw { }

	[MapObjectProperty]
	public class SawProperty : IMapObjectProperty<IMapObjectSaw>
	{
		public virtual void Serialize(GameObject instance, IMapObjectSaw target) { }

		public virtual void Deserialize(IMapObjectSaw data, GameObject target)
		{
			target.GetOrAddComponent<SawTransformer>();
		}
	}
}
