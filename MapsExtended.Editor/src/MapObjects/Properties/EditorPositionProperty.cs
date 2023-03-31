using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorPropertySerializer(typeof(PositionProperty))]
	public class EditorPositionPropertySerializer : PositionPropertySerializer
	{
		public override void Deserialize(PositionProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<PositionHandler>();
			target.GetOrAddComponent<SelectionHandler>();
		}
	}

	[PropertyInspector(typeof(PositionProperty))]
	public class PositionElement : Vector2Element
	{
		protected override Vector2 Value
		{
			get => this.Context.InspectorTarget.GetHandlerValue<PositionProperty>();
			set => this.Context.InspectorTarget.SetHandlerValue<PositionProperty>(value);
		}

		public PositionElement() : base("Position") { }
	}
}
