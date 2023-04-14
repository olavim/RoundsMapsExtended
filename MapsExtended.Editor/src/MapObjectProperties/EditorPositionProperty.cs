using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(PositionProperty))]
	public class EditorPositionPropertySerializer : PositionPropertySerializer
	{
		public override void Deserialize(PositionProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<PositionHandler>();
		}
	}

	[PropertyInspector(typeof(PositionProperty))]
	public class PositionElement : Vector2Element
	{
		public PositionElement() : base("Position") { }

		protected override Vector2 GetValue() => this.Context.InspectorTarget.GetHandlerValue<PositionProperty>();

		protected override void OnChange(Vector2 value)
		{
			this.Context.InspectorTarget.SetHandlerValue<PositionProperty>(value);
			this.Context.Editor.RefreshHandlers();
			this.Context.Editor.TakeSnaphot();
		}
	}
}
