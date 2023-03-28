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
	[EditorPropertySerializer]
	public class EditorRopePositionPropertySerializer : PropertySerializer<RopePositionProperty>, IInspectable
	{
		public override void Serialize(GameObject instance, RopePositionProperty property)
		{
			var ropeInstance = instance.GetComponent<EditorRopeInstance>();
			property.StartPosition = ropeInstance.GetAnchor(0).transform.position;
			property.EndPosition = ropeInstance.GetAnchor(1).transform.position;
		}

		public override void Deserialize(RopePositionProperty property, GameObject target)
		{
			target.transform.GetChild(0).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(0).gameObject.GetOrAddComponent<RopeAnchorPositionHandler>();
			target.transform.GetChild(0).gameObject.GetOrAddComponent<SelectionHandler>();

			target.transform.GetChild(1).gameObject.GetOrAddComponent<MapObjectAnchor>();
			target.transform.GetChild(1).gameObject.GetOrAddComponent<RopeAnchorPositionHandler>();
			target.transform.GetChild(1).gameObject.GetOrAddComponent<SelectionHandler>();

			var startCollider = target.transform.GetChild(0).gameObject.GetOrAddComponent<BoxCollider2D>();
			var endCollider = target.transform.GetChild(1).gameObject.GetOrAddComponent<BoxCollider2D>();
			startCollider.size = Vector2.one * 1;
			endCollider.size = Vector2.one * 1;

			target.GetOrAddComponent<EditorRopeInstance>();
			target.GetOrAddComponent<Visualizers.RopeVisualizer>();

			target.transform.GetChild(0).GetComponent<RopeAnchorPositionHandler>().SetValue(property.StartPosition);
			target.transform.GetChild(1).GetComponent<RopeAnchorPositionHandler>().SetValue(property.EndPosition);
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Anchor Position 1")
				.ValueSetter(value => this.GetAnchorHandler(inspector.target, 0).SetValue(value))
				.ValueGetter(() => inspector.target.GetComponent<EditorRopeInstance>().GetAnchor(0).GetAnchoredPosition());

			builder.Property<Vector2>("Anchor Position 2")
				.ValueSetter(value => this.GetAnchorHandler(inspector.target, 1).SetValue(value))
				.ValueGetter(() => inspector.target.GetComponent<EditorRopeInstance>().GetAnchor(1).GetAnchoredPosition());
		}

		private RopeAnchorPositionHandler GetAnchorHandler(MapObjectInstance target, int anchor)
		{
			return target.GetComponent<EditorRopeInstance>().GetAnchor(anchor).GetComponent<RopeAnchorPositionHandler>();
		}
	}

	public class EditorRopeInstance : MonoBehaviour
	{
		private List<MapObjectAnchor> anchors;

		protected virtual void Awake()
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
