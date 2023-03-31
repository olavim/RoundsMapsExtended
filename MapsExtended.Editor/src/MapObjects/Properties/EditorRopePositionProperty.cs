using MapsExt.MapObjects.Properties;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.Editor.MapObjects.Properties;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.MapObjects
{
	[EditorPropertySerializer(typeof(RopePositionProperty))]
	public class EditorRopePositionPropertySerializer : PropertySerializer<RopePositionProperty>
	{
		public override RopePositionProperty Serialize(GameObject instance)
		{
			var ropeInstance = instance.GetComponent<EditorRopeInstance>();
			return new RopePositionProperty()
			{
				StartPosition = ropeInstance.GetAnchor(0).GetAnchoredPosition(),
				EndPosition = ropeInstance.GetAnchor(1).GetAnchoredPosition()
			};
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
	}

	[PropertyInspector(typeof(RopePositionProperty))]
	public class RopePositionElement : InspectorElement
	{
		private Vector2Input _input1;
		private Vector2Input _input2;

		protected override GameObject GetInstance()
		{
			var instance = new GameObject("RopePositionGroup");
			var layoutGroup = instance.AddComponent<VerticalLayoutGroup>();
			layoutGroup.childControlWidth = true;
			layoutGroup.childControlHeight = true;
			layoutGroup.childForceExpandWidth = true;

			var pos1Instance = GameObject.Instantiate(Assets.InspectorVector2Prefab, instance.transform);
			var input1 = pos1Instance.GetComponent<InspectorVector2>();
			input1.Label.text = "Anchor Position 1";
			this._input1 = input1.Input;
			this._input1.OnChanged += value => this.HandleInputChange(value, 0);

			var pos2Instance = GameObject.Instantiate(Assets.InspectorVector2Prefab, instance.transform);
			var input2 = pos2Instance.GetComponent<InspectorVector2>();
			input2.Label.text = "Anchor Position 2";
			this._input2 = input2.Input;
			this._input2.OnChanged += value => this.HandleInputChange(value, 1);

			return instance;
		}

		public override void OnUpdate()
		{
			var ropeInstance = this.Context.InspectorTarget.GetComponent<EditorRopeInstance>();
			this._input1.SetWithoutEvent(ropeInstance.GetAnchor(0).GetAnchoredPosition());
			this._input2.SetWithoutEvent(ropeInstance.GetAnchor(1).GetAnchoredPosition());
		}

		private void HandleInputChange(Vector2 value, int anchor)
		{
			var ropeInstance = this.Context.InspectorTarget.GetComponent<EditorRopeInstance>();
			ropeInstance.GetAnchor(anchor).GetComponent<RopeAnchorPositionHandler>().SetValue(value);
			this.Context.Editor.TakeSnaphot();
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
