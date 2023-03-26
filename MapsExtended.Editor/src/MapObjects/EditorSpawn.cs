using MapsExt.MapObjects;
using MapsExt.Editor.MapObjects.Properties;
using UnityEngine;
using UnboundLib;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObject("Spawn")]
	public class EditorSpawn : Spawn
	{
		public override GameObject Prefab
		{
			get
			{
				var prefab = base.Prefab;
				prefab.GetOrAddComponent<Visualizers.SpawnVisualizer>();
				return prefab;
			}
		}
	}

	[EditorMapObjectPropertySerializer]
	public class EditorSpawnIDPropertySerializer : SpawnIDPropertySerializer { }
}
