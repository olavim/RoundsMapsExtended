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
			target.GetOrAddComponent<SpatialInspectorSpec>();
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
		[MapObjectInspector.Vector2Property("Position", typeof(MoveCommand))]
		public Vector2 position => this.transform.position;
		[MapObjectInspector.Vector2Property("Size", typeof(ResizeCommand))]
		public Vector2 size => this.transform.localScale;
		[MapObjectInspector.QuaternionProperty("Rotation", typeof(RotateCommand))]
		public Quaternion rotation => this.transform.rotation;

		private Action onUpdate;

		[MapObjectInspector.ButtonBuilder]
		public GameObject AnimationButton(MapEditor editor, MapEditorUI editorUI)
		{
			var instance = GameObject.Instantiate(Assets.InspectorButtonPrefab);
			var button = instance.GetComponent<InspectorButton>().button;

			button.onClick.AddListener(() =>
			{
				if (editorUI.animationWindow.gameObject.activeSelf)
				{
					editorUI.animationWindow.Close();
				}
				else
				{
					editorUI.animationWindow.Open();
				}
			});

			this.onUpdate += () =>
			{
				if (button)
				{
					bool animWindowOpen = editorUI.animationWindow.gameObject.activeSelf;
					button.GetComponentInChildren<Text>().text = animWindowOpen ? "Close Animation" : "Edit Animation";
				}
			};

			return instance;
		}

		private void Update()
		{
			this.onUpdate?.Invoke();
		}
	}
}
