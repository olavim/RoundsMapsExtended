using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;
using MapsExt.Editor.Commands;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObjectSpec(typeof(Rope), "Rope")]
	public static class EditorRopeSpec
	{
		[EditorMapObjectPrefab]
		public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");

		[EditorMapObjectSerializer]
		public static void Serialize(GameObject instance, Rope target)
		{
			var ropeInstance = instance.GetComponent<EditorRopeInstance>();
			target.startPosition = ropeInstance.GetAnchor(0).GetPosition();
			target.endPosition = ropeInstance.GetAnchor(1).GetPosition();
		}

		[EditorMapObjectDeserializer]
		public static void Deserialize(Rope data, GameObject target)
		{
			target.transform.GetChild(0).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(0).gameObject.GetOrAddComponent<RopeAnchorActionHandler>();

			target.transform.GetChild(1).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(1).gameObject.GetOrAddComponent<RopeAnchorActionHandler>();

			var startCollider = target.transform.GetChild(0).gameObject.GetOrAddComponent<BoxCollider2D>();
			var endCollider = target.transform.GetChild(1).gameObject.GetOrAddComponent<BoxCollider2D>();
			startCollider.size = Vector2.one * 1;
			endCollider.size = Vector2.one * 1;

			target.GetOrAddComponent<RopeInspectorSpec>();

			var instance = target.GetOrAddComponent<EditorRopeInstance>();
			target.GetOrAddComponent<Visualizers.RopeVisualizer>();

			instance.Detach();
			target.transform.GetChild(0).position = data.startPosition;
			target.transform.GetChild(1).position = data.endPosition;
			instance.UpdateAttachments();
		}
	}

	public class RopeInspectorSpec : InspectorSpec
	{
		[MapObjectInspector.Vector2Property("Anchor Position 1", typeof(MoveCommand), handlerIndex = 0)]
		public Vector2 position1 => this.GetComponent<EditorRopeInstance>().GetAnchor(0).GetPosition();
		[MapObjectInspector.Vector2Property("Anchor Position 2", typeof(MoveCommand), handlerIndex = 1)]
		public Vector2 position2 => this.GetComponent<EditorRopeInstance>().GetAnchor(1).GetPosition();
	}

	public class EditorRopeInstance : MonoBehaviour
	{
		private List<MapObjectAnchor> anchors;

		private void Awake()
		{
			this.anchors = this.gameObject.GetComponentsInChildren<MapObjectAnchor>().ToList();
		}

		public MapObjectAnchor GetAnchor(int index)
		{
			return this.anchors[index];
		}

		public void UpdateAttachments(bool allowDetach = true)
		{
			foreach (var anchor in this.anchors)
			{
				anchor.UpdateAttachment(allowDetach);
			}
		}

		public void Detach()
		{
			foreach (var anchor in this.anchors)
			{
				anchor.Detach();
			}
		}
	}
}
