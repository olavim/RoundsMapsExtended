using UnityEngine;
using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using UnboundLib;
using MapsExt.Editor.UI;
using MapsExt.Editor.Commands;
using UnityEngine.UI;
using System;

namespace MapsExt.Editor.MapObjects
{
	public static class EditorSpatialSerializer
	{
		public static void Serialize(GameObject instance, SpatialMapObject target) { }

		public static void Deserialize(SpatialMapObject data, GameObject target)
		{
			target.GetOrAddComponent<SpatialActionHandler>();
		}

		// Helper methods to make simple editor map object specs less verbose to write
		internal static SerializerAction<T> BuildSerializer<T>(SerializerAction<T> action) where T : SpatialMapObject
		{
			SerializerAction<T> result = null;

			result += (instance, target) => EditorSpatialSerializer.Serialize(instance, (T) target);
			result += (instance, target) => action(instance, (T) target);

			return result;
		}

		internal static DeserializerAction<T> BuildDeserializer<T>(DeserializerAction<T> action) where T : SpatialMapObject
		{
			DeserializerAction<T> result = null;

			result += (data, target) => EditorSpatialSerializer.Deserialize((T) data, target);
			result += (data, target) => action((T) data, target);

			return result;
		}
	}

	public class SpatialInspectorSpec : InspectorSpec
	{
		public override void OnInspectorLayout(InspectorLayoutBuilder builder, MapEditor editor, MapEditorUI editorUI)
		{
			builder.Property<Vector2>("Position")
				.CommandGetter(value => new MoveCommand(this.GetComponent<EditorActionHandler>(), this.transform.position, value))
				.ValueGetter(() => this.transform.position)
				.ChangeEvent(() => editor.UpdateRopeAttachments());

			builder.Property<Vector2>("Size")
				.CommandGetter(value => new ResizeCommand(this.GetComponent<EditorActionHandler>(), this.transform.localScale, value))
				.ValueGetter(() => this.transform.localScale)
				.ChangeEvent(() => editor.UpdateRopeAttachments());

			builder.Property<Quaternion>("Rotation")
				.CommandGetter(value => new RotateCommand(this.GetComponent<EditorActionHandler>(), this.transform.rotation, value))
				.ValueGetter(() => this.transform.rotation)
				.ChangeEvent(() => editor.UpdateRopeAttachments());

			builder.Divider();

			builder.Button()
				.ClickEvent(() => editorUI.animationWindow.SetOpen(!editorUI.animationWindow.gameObject.activeSelf))
				.UpdateEvent(button => button.GetComponentInChildren<Text>().text = editorUI.animationWindow.gameObject.activeSelf ? "Close Animation" : "Edit Animation");
		}
	}
}
