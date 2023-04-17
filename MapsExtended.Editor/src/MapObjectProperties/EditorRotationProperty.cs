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
	public class RotationElement : FloatElement
	{
		public RotationElement() : base("Rotation") { }

		protected override void OnChange(float angle, ChangeType changeType)
		{
			if (changeType == ChangeType.Change || changeType == ChangeType.ChangeEnd)
			{
				this.Context.InspectorTarget.SetEditorMapObjectProperty<RotationProperty>(EditorUtils.Snap(angle, 5));
			}

			if (changeType == ChangeType.ChangeEnd)
			{
				this.Context.Editor.RefreshHandlers();
				this.Context.Editor.TakeSnaphot();
			}
		}

		protected override float GetValue() => this.Context.InspectorTarget.GetEditorMapObjectProperty<RotationProperty>();
	}
}
