using MapsExt.Editor.Events;
using MapsExt.Editor.UI;
using MapsExt.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(ScaleProperty))]
	public class EditorScalePropertySerializer : ScalePropertySerializer, IPropertyReader<ScaleProperty>
	{
		public override void WriteProperty(ScaleProperty property, GameObject target)
		{
			base.WriteProperty(property, target);
			target.GetOrAddComponent<SizeHandler>();
		}

		public virtual ScaleProperty ReadProperty(GameObject instance)
		{
			return instance.transform.localScale;
		}
	}

	[InspectorElement(typeof(ScaleProperty))]
	public class ScaleElement : Vector2Element
	{
		public ScaleElement() : base("Size") { }

		protected override Vector2 GetValue() => this.Context.InspectorTarget.ReadProperty<ScaleProperty>();

		protected override void OnChange(Vector2 value)
		{
			this.Context.InspectorTarget.WriteProperty<ScaleProperty>(value);
			this.Context.Editor.TakeSnaphot();
		}
	}
}
