using MapsExt.Editor.UI;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorMapObjectProperty]
	public class EditorScaleProperty : ScaleProperty, IInspectable
	{
		public override void Deserialize(IMapObjectScale data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<ActionHandlers.SizeHandler>();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Size")
				.ValueSetter(value => inspector.selectedObject.GetComponent<ActionHandlers.SizeHandler>().SetSize(value))
				.OnChange(value => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.selectedObject.transform.localScale);
		}
	}
}
