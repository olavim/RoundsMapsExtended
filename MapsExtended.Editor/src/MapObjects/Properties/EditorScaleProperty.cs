using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorPropertySerializer(typeof(ScaleProperty))]
	public class EditorScalePropertySerializer : ScalePropertySerializer
	{
		public override void Deserialize(ScaleProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<SizeHandler>();
		}
	}

	[PropertyInspector(typeof(ScaleProperty))]
	public class ScaleElement : Vector2Element
	{
		protected override Vector2 Value
		{
			get => this.Context.InspectorTarget.GetHandlerValue<ScaleProperty>();
			set => this.Context.InspectorTarget.SetHandlerValue<ScaleProperty>(value);
		}

		public ScaleElement() : base("Size") { }
	}
}
