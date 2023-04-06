using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(RotationProperty))]
	public class EditorRotationPropertySerializer : RotationPropertySerializer
	{
		public override void Deserialize(RotationProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<ActionHandlers.RotationHandler>();
		}
	}

	[PropertyInspector(typeof(RotationProperty))]
	public class RotationElement : QuaternionElement
	{
		public override Quaternion Value
		{
			get => this.Context.InspectorTarget.GetHandlerValue<RotationProperty>();
			set => this.Context.InspectorTarget.SetHandlerValue<RotationProperty>(value);
		}

		public RotationElement() : base("Rotation") { }
	}
}
