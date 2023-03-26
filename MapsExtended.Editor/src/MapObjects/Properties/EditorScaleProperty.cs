using MapsExt.Editor.UI;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorMapObjectPropertySerializer]
	public class EditorScalePropertySerializer : ScalePropertySerializer, IInspectable
	{
		public override void Deserialize(ScaleProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<ActionHandlers.SizeHandler>();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Size")
				.ValueSetter(value => inspector.target.GetComponent<ActionHandlers.SizeHandler>().SetValue(value))
				.OnChange(_ => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.target.transform.localScale);
		}
	}
}
