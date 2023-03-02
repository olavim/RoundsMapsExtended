using MapsExt.Editor.UI;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorMapObjectProperty]
	public class EditorRotationProperty : RotationProperty, IInspectable
	{
		public override void Deserialize(IMapObjectRotation data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<ActionHandlers.RotationHandler>();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Quaternion>("Rotation")
				.ValueSetter(value => inspector.selectedObject.GetComponent<ActionHandlers.RotationHandler>().SetRotation(value))
				.OnChange(value => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.selectedObject.transform.rotation);
		}
	}
}
