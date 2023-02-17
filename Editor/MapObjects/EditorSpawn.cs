using MapsExt.MapObjects;
using MapsExt.Editor.MapObjects.Properties;
using UnityEngine;
using UnboundLib;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObject("Spawn")]
	public class EditorSpawn : Spawn { }

	[EditorMapObjectProperty]
	public class EditorSpawnProperty : SpawnProperty
	{
		public override void Deserialize(SpawnData data, GameObject target)
		{
			base.Deserialize(data, target);
			target.gameObject.GetOrAddComponent<Visualizers.SpawnVisualizer>();
			target.transform.SetAsLastSibling();
		}
	}
}
