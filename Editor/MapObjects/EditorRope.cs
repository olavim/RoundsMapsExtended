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
	[EditorMapObjectBlueprint("Rope")]
	public class EditorRopeBP : BaseMapObjectBlueprint<Rope>, IInspectable
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");

		public override void Serialize(GameObject instance, Rope target)
		{
			var ropeInstance = instance.GetComponent<EditorRopeInstance>();
			target.startPosition = ropeInstance.GetAnchor(0).transform.position;
			target.endPosition = ropeInstance.GetAnchor(1).transform.position;
		}

		public override void Deserialize(Rope data, GameObject target)
		{
			var anchor1 = target.transform.GetChild(0).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(0).gameObject.GetOrAddComponent<RopeAnchorMoveHandler>();

			var anchor2 = target.transform.GetChild(1).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(1).gameObject.GetOrAddComponent<RopeAnchorMoveHandler>();

			anchor1.autoUpdatePosition = false;
			anchor2.autoUpdatePosition = false;

			var startCollider = target.transform.GetChild(0).gameObject.GetOrAddComponent<BoxCollider2D>();
			var endCollider = target.transform.GetChild(1).gameObject.GetOrAddComponent<BoxCollider2D>();
			startCollider.size = Vector2.one * 1;
			endCollider.size = Vector2.one * 1;

			var instance = target.GetOrAddComponent<EditorRopeInstance>();
			target.GetOrAddComponent<Visualizers.RopeVisualizer>();

			target.transform.GetChild(0).position = data.startPosition;
			target.transform.GetChild(1).position = data.endPosition;
			instance.UpdateAttachments();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Anchor Position 1")
				.ValueSetter(value => inspector.target.GetComponent<EditorRopeInstance>().GetAnchor(0).transform.position = value)
				.ValueGetter(() => inspector.target.GetComponent<EditorRopeInstance>().GetAnchor(0).transform.position);
			builder.Property<Vector2>("Anchor Position 2")
				.ValueSetter(value => inspector.target.GetComponent<EditorRopeInstance>().GetAnchor(1).transform.position = value)
				.ValueGetter(() => inspector.target.GetComponent<EditorRopeInstance>().GetAnchor(1).transform.position);
		}
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

		public void UpdateAttachments()
		{
			foreach (var anchor in this.anchors)
			{
				anchor.UpdateAttachment();
			}
		}
	}
}
