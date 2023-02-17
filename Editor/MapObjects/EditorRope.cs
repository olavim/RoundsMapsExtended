using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.Editor.MapObjects.Properties;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects
{
	[EditorMapObject("Rope")]
	public class EditorRope : IMapObject<RopeData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Editor Rope");
	}

	[EditorMapObjectProperty]
	public class EditorRopeProperty : IMapObjectProperty<RopeData>, IInspectable
	{
		public void Serialize(GameObject instance, RopeData target)
		{
			var ropeInstance = instance.GetComponent<EditorRopeInstance>();
			target.startPosition = ropeInstance.GetAnchor(0).transform.position;
			target.endPosition = ropeInstance.GetAnchor(1).transform.position;
		}

		public void Deserialize(RopeData data, GameObject target)
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
				.ValueSetter(value => this.GetAnchorHandler(inspector.target, 0).SetPosition(value))
				.ValueGetter(() => inspector.target.GetComponent<EditorRopeInstance>().GetAnchor(0).transform.position);

			builder.Property<Vector2>("Anchor Position 2")
				.ValueSetter(value => this.GetAnchorHandler(inspector.target, 1).SetPosition(value))
				.ValueGetter(() => inspector.target.GetComponent<EditorRopeInstance>().GetAnchor(1).transform.position);
		}

		private RopeAnchorMoveHandler GetAnchorHandler(MapObjectInstance target, int anchor)
		{
			return target.GetComponent<EditorRopeInstance>().GetAnchor(anchor).GetComponent<RopeAnchorMoveHandler>();
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
