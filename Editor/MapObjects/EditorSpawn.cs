using MapsExt.MapObjects;
using UnityEngine;
using UnboundLib;

namespace MapsExt.Editor.MapObjects
{
	[MapsExtendedEditorMapObject(typeof(Spawn), "Spawn")]
	public class EditorSpawnSpecification : SpawnSpecification
	{
		protected override void OnDeserialize(Spawn data, GameObject target)
		{
			base.OnDeserialize(data, target);
			target.gameObject.GetOrAddComponent<Visualizers.SpawnVisualizer>();
			target.gameObject.GetOrAddComponent<SpawnActionHandler>();
			target.transform.SetAsLastSibling();
		}
	}
}
