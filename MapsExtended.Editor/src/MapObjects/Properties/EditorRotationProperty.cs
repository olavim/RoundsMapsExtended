using MapsExt.Editor.UI;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorMapObjectPropertySerializer]
	public class EditorRotationPropertySerializer : RotationPropertySerializer, IInspectable
	{
		public override void Deserialize(RotationProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<ActionHandlers.RotationHandler>();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Quaternion>("Rotation")
				.ValueSetter(value => inspector.target.GetComponent<ActionHandlers.RotationHandler>().SetValue(value))
				.OnChange(_ => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.target.transform.rotation);
		}
	}
}
