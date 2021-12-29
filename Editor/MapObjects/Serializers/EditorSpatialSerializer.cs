using UnityEngine;
using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using UnboundLib;
using MapsExt.Editor.UI;
using MapsExt.Editor.Commands;
using UnityEngine.UI;

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
			builder.Property<Vector2>(
				"Position",
				value => new MoveCommand(this.GetComponent<EditorActionHandler>(), this.transform.position, value),
				() => this.transform.position
			);

			builder.Property<Vector2>(
				"Size",
				value => new ResizeCommand(this.GetComponent<EditorActionHandler>(), this.transform.localScale, value),
				() => this.transform.localScale
			);

			builder.Property<Quaternion>(
				"Rotation",
				value => new RotateCommand(this.GetComponent<EditorActionHandler>(), this.transform.rotation, value),
				() => this.transform.rotation
			);

			builder.Divider();

			builder.Button(
				() =>
				{
					if (editorUI.animationWindow.gameObject.activeSelf)
					{
						editorUI.animationWindow.Close();
					}
					else
					{
						editorUI.animationWindow.Open();
					}
				},
				button =>
				{
					bool animWindowOpen = editorUI.animationWindow.gameObject.activeSelf;
					button.GetComponentInChildren<Text>().text = animWindowOpen ? "Close Animation" : "Edit Animation";
				}
			);
		}
	}
}
