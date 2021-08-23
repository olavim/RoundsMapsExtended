using UnityEngine;
using MapsExt.MapObjects;

namespace MapsExt.Editor.MapObjects
{
	public interface IEditorMapObjectSpecification : IMapObjectSpecification {
		new GameObject Prefab { get; }

		new void Deserialize(MapObject data, GameObject target);

		new void Serialize(GameObject instance, MapObject target);
	}
}
