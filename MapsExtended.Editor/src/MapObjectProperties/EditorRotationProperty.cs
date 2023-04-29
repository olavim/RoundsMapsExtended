using MapsExt.Editor.UI;
using MapsExt.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(RotationProperty))]
	public class EditorRotationPropertySerializer : RotationPropertySerializer, IPropertyReader<RotationProperty>
	{
		public override void WriteProperty(RotationProperty property, GameObject target)
		{
			base.WriteProperty(property, target);
			target.GetOrAddComponent<Events.RotationHandler>();
		}

		public virtual RotationProperty ReadProperty(GameObject instance)
		{
			return instance.GetComponent<RotationPropertyInstance>().Rotation;
		}
	}

	[InspectorElement(typeof(RotationProperty))]
	public class RotationElement : FloatElement
	{
		public RotationElement() : base("Rotation", 0, 360) { }

		protected override void OnChange(float angle, ChangeType changeType)
		{
			if (changeType == ChangeType.Change || changeType == ChangeType.ChangeEnd)
			{
				this.Context.InspectorTarget.WriteProperty<RotationProperty>(EditorUtils.Snap(angle, 5));
			}

			if (changeType == ChangeType.ChangeEnd)
			{
				this.Context.Editor.TakeSnaphot();
			}
		}

		protected override float GetValue() => this.Context.InspectorTarget.ReadProperty<RotationProperty>();
	}
}
