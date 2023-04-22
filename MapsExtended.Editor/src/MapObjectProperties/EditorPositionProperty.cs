using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(PositionProperty))]
	public class EditorPositionPropertySerializer : PositionPropertySerializer, IPropertyReader<PositionProperty>
	{
		public override void WriteProperty(PositionProperty property, GameObject target)
		{
			base.WriteProperty(property, target);
			target.GetOrAddComponent<PositionHandler>();
		}

		public virtual PositionProperty ReadProperty(GameObject instance)
		{
			return instance.transform.position;
		}
	}

	[PropertyInspector(typeof(PositionProperty))]
	public class PositionElement : Vector2Element
	{
		public PositionElement() : base("Position") { }

		protected override Vector2 GetValue() => this.Context.InspectorTarget.ReadProperty<PositionProperty>();

		protected override void OnChange(Vector2 value)
		{
			this.Context.InspectorTarget.WriteProperty<PositionProperty>(value);
			this.Context.Editor.RefreshHandlers();
			this.Context.Editor.TakeSnaphot();
		}
	}
}
