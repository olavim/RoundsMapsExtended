using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using UnityEngine;
using UnboundLib;
using MapsExt.Editor.UI;
using MapsExt.Editor.Commands;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObjectSpec(typeof(Spawn), "Spawn")]
	[EditorInspectorSpec(typeof(SpawnInspectorSpec))]
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

	public class SpawnInspectorSpec : InspectorSpec
	{
		public override void OnInspectorLayout(InspectorLayoutBuilder builder, MapEditor editor, MapEditorUI editorUI)
		{
			builder.Property<Vector2>(
				"Position",
				value => new MoveCommand(this.GetComponent<EditorActionHandler>(), this.transform.position, value),
				() => this.transform.position
			);
		}
	}
}
