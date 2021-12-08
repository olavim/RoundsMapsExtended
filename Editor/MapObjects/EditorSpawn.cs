using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using UnityEngine;
using UnboundLib;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObjectSpec(typeof(Spawn), "Spawn")]
	public static class EditorSpawnSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => SpawnSpec.Prefab;

		[EditorMapObjectSerializer]
		public static void Serialize(GameObject instance, Spawn target)
		{
			SpawnSpec.Serialize(instance, target);
		}

		[EditorMapObjectDeserializer]
		public static void Deserialize(Spawn data, GameObject target)
		{
			SpawnSpec.Deserialize(data, target);
			target.gameObject.GetOrAddComponent<Visualizers.SpawnVisualizer>();
			target.gameObject.GetOrAddComponent<SpawnActionHandler>();
			target.transform.SetAsLastSibling();
		}
	}
}
