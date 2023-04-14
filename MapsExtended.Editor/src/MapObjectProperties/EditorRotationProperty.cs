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
		public RotationElement() : base("Rotation") { }

		protected override void OnChange(Quaternion rotation, ChangeType changeType)
		{
			if (changeType == ChangeType.Change || changeType == ChangeType.ChangeEnd)
			{
				this.Context.InspectorTarget.SetHandlerValue<RotationProperty>(rotation);
			}

			if (changeType == ChangeType.ChangeEnd)
			{
				this.Context.Editor.RefreshHandlers();
				this.Context.Editor.TakeSnaphot();
			}
		}

		protected override Quaternion GetValue() => this.Context.InspectorTarget.GetHandlerValue<RotationProperty>();
	}
}
