using MapsExt.MapObjects;
using UnityEngine;
using UnboundLib;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(Spawn), "Spawn")]
	public class EditorSpawnSpecification : SpawnSpecification, IEditorMapObjectSpecification
	{
		protected override void Deserialize(Spawn data, GameObject target)
		{
			base.Deserialize(data, target);
			target.gameObject.GetOrAddComponent<Visualizers.SpawnVisualizer>();
			target.gameObject.GetOrAddComponent<SpawnActionHandler>();
			target.transform.SetAsLastSibling();
		}
	}
}
