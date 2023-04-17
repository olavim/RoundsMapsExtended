using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.Properties
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
		public ScaleElement() : base("Size") { }

		protected override Vector2 GetValue() => this.Context.InspectorTarget.GetEditorMapObjectProperty<ScaleProperty>();

		protected override void OnChange(Vector2 value)
		{
			this.Context.InspectorTarget.SetEditorMapObjectProperty<ScaleProperty>(value);
			this.Context.Editor.RefreshHandlers();
			this.Context.Editor.TakeSnaphot();
		}
	}
}
