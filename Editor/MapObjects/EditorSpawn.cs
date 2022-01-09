﻿using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using UnityEngine;
using UnboundLib;
using MapsExt.Editor.UI;
using MapsExt.Editor.Commands;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObjectBlueprint("Spawn")]
	public class EditorSpawnBP : BaseEditorMapObjectBlueprint<Spawn>, IInspectable
	{
		public override void Serialize(GameObject instance, Spawn target)
		{
			this.baseBlueprint.Serialize(instance, target);
		}

		public override void Deserialize(Spawn data, GameObject target)
		{
			this.baseBlueprint.Deserialize(data, target);
			target.gameObject.GetOrAddComponent<Visualizers.SpawnVisualizer>();
			target.gameObject.GetOrAddComponent<MoveActionHandler>();
			target.transform.SetAsLastSibling();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Position")
				.ValueSetter(value => inspector.selectedObject.transform.position = value)
				.ValueGetter(() => inspector.selectedObject.transform.position);
		}
	}
}
