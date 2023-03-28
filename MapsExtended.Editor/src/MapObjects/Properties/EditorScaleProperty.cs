using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorPropertySerializer]
	public class EditorScalePropertySerializer : ScalePropertySerializer, IInspectable
	{
		public override void Deserialize(ScaleProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<SizeHandler>();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Size")
				.ValueSetter(value => inspector.target.SetHandlerValue<ScaleProperty>(value))
				.OnChange(_ => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.target.GetHandlerValue<ScaleProperty>());
		}
	}
}
