using MapsExt.Properties;
using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using UnityEngine;
using UnityEngine.UI;
using MapsExt.Editor.MapObjects;
using UnboundLib;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(RopePositionProperty))]
	public class EditorRopePositionPropertySerializer : PropertySerializer<RopePositionProperty>
	{
		public override RopePositionProperty Serialize(GameObject instance)
		{
			var ropeInstance = instance.GetComponent<EditorRope.RopeInstance>();
			return new RopePositionProperty()
			{
				StartPosition = ropeInstance.GetAnchor(0).GetAnchoredPosition(),
				EndPosition = ropeInstance.GetAnchor(1).GetAnchoredPosition()
			};
		}

		public override void Deserialize(RopePositionProperty property, GameObject target)
		{
			var ropeInstance = target.GetComponent<EditorRope.RopeInstance>();

			ropeInstance.GetAnchor(0).gameObject.GetOrAddComponent<RopeAnchorPositionHandler>();
			ropeInstance.GetAnchor(0).gameObject.GetOrAddComponent<SelectionHandler>();

			ropeInstance.GetAnchor(1).gameObject.GetOrAddComponent<RopeAnchorPositionHandler>();
			ropeInstance.GetAnchor(1).gameObject.GetOrAddComponent<SelectionHandler>();

			ropeInstance.GetAnchor(0).SetHandlerValue<PositionProperty>(property.StartPosition);
			ropeInstance.GetAnchor(1).SetHandlerValue<PositionProperty>(property.EndPosition);
		}
	}

	[PropertyInspector(typeof(RopePositionProperty))]
	public class RopePositionElement : InspectorElement
	{
		private Vector2Input _input1;
		private Vector2Input _input2;

		public Vector2 AnchorPosition1
		{
			get => this.Context.InspectorTarget.GetComponent<EditorRope.RopeInstance>().GetAnchor(0).GetAnchoredPosition();
			set => this.HandleInputChange(value, 0);
		}

		public Vector2 AnchorPosition2
		{
			get => this.Context.InspectorTarget.GetComponent<EditorRope.RopeInstance>().GetAnchor(1).GetAnchoredPosition();
			set => this.HandleInputChange(value, 1);
		}

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
			this._input1.SetWithoutEvent(this.AnchorPosition1);
			this._input2.SetWithoutEvent(this.AnchorPosition2);
		}

		private void HandleInputChange(Vector2 value, int anchor)
		{
			var ropeInstance = this.Context.InspectorTarget.GetComponent<EditorRope.RopeInstance>();
			ropeInstance.GetAnchor(anchor).SetHandlerValue<PositionProperty>(value);
			this.Context.Editor.TakeSnaphot();
		}
	}
}
