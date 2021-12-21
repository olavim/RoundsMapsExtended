using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using UnityEngine;
using UnboundLib;
using MapsExt.Editor.UI;
using MapsExt.Editor.Commands;

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
			target.gameObject.GetOrAddComponent<SpawnInspectorSpec>();
			target.transform.SetAsLastSibling();
		}
	}

	public class SpawnInspectorSpec : InspectorSpec
	{
		[MapObjectInspector.Vector2Property("Position", typeof(MoveCommand))]
		public Vector2 position => this.transform.position;
	}
}
