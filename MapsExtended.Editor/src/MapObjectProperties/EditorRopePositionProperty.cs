using MapsExt.Properties;
using MapsExt.Editor.UI;
using UnityEngine;
using UnityEngine.UI;
using MapsExt.Editor.MapObjects;
using MapsExt.Editor.Events;
using UnboundLib;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(RopePositionProperty))]
	public class EditorRopePositionPropertySerializer : IPropertySerializer<RopePositionProperty>
	{
		public virtual void WriteProperty(RopePositionProperty property, GameObject target)
		{
			var ropeInstance = target.GetComponent<EditorRope.RopeInstance>();

			ropeInstance.GetAnchor(0).Detach();
			ropeInstance.GetAnchor(0).transform.position = property.StartPosition;
			ropeInstance.GetAnchor(0).UpdateAttachment();
			ropeInstance.GetAnchor(0).gameObject.GetOrAddComponent<RopeAnchorPositionHandler>();

			ropeInstance.GetAnchor(1).Detach();
			ropeInstance.GetAnchor(1).transform.position = property.EndPosition;
			ropeInstance.GetAnchor(1).UpdateAttachment();
			ropeInstance.GetAnchor(1).gameObject.GetOrAddComponent<RopeAnchorPositionHandler>();
		}

		public virtual RopePositionProperty ReadProperty(GameObject instance)
		{
			var ropeInstance
				= instance.GetComponent<EditorRope.RopeInstance>()
				?? throw new System.ArgumentException("GameObject does not have a rope instance", nameof(instance));

			return new RopePositionProperty()
			{
				StartPosition = ropeInstance.GetAnchor(0).GetAnchoredPosition(),
				EndPosition = ropeInstance.GetAnchor(1).GetAnchoredPosition()
			};
		}
	}

	[InspectorElement(typeof(RopePositionProperty))]
	public class RopePositionElement : InspectorElement
	{
		private Vector2Input _input1;
		private Vector2Input _input2;

		public RopePositionProperty Value
		{
			get => new(
				this.Context.InspectorTarget.GetComponent<EditorRope.RopeInstance>().GetAnchor(0).GetAnchoredPosition(),
				this.Context.InspectorTarget.GetComponent<EditorRope.RopeInstance>().GetAnchor(1).GetAnchoredPosition()
			);
			set => this.HandleInputChange(value);
		}

		protected override GameObject GetInstance()
		{
			var instance = new GameObject("RopePositionGroup");
			var layoutGroup = instance.AddComponent<VerticalLayoutGroup>();
			layoutGroup.childControlWidth = true;
			layoutGroup.childControlHeight = true;
			layoutGroup.childForceExpandWidth = true;

			var pos1Instance = GameObject.Instantiate(Assets.InspectorVector2InputPrefab, instance.transform);
			var input1 = pos1Instance.GetComponent<InspectorVector2Input>();
			input1.Label.text = "Anchor Position 1";
			this._input1 = input1.Input;
			this._input1.OnChanged += value => this.HandleInputChange(new(value, this.Value.EndPosition));

			var pos2Instance = GameObject.Instantiate(Assets.InspectorVector2InputPrefab, instance.transform);
			var input2 = pos2Instance.GetComponent<InspectorVector2Input>();
			input2.Label.text = "Anchor Position 2";
			this._input2 = input2.Input;
			this._input2.OnChanged += value => this.HandleInputChange(new(this.Value.StartPosition, value));

			return instance;
		}

		public override void OnUpdate()
		{
			var val = this.Value;
			this._input1.SetWithoutEvent(val.StartPosition);
			this._input2.SetWithoutEvent(val.EndPosition);
		}

		private void HandleInputChange(RopePositionProperty value)
		{
			this.Context.InspectorTarget.WriteProperty(value);
			this.Context.Editor.TakeSnaphot();
		}
	}
}
