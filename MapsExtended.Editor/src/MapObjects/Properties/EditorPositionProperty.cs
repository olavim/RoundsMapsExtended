using MapsExt.Editor.ActionHandlers;
using MapsExt.Editor.UI;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorPropertySerializer]
	public class EditorPositionPropertySerializer : PositionPropertySerializer, IInspectable
	{
		public override void Deserialize(PositionProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<PositionHandler>();
			target.GetOrAddComponent<SelectionHandler>();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Position")
				.ValueSetter(value => inspector.target.SetHandlerValue<PositionProperty>(value))
				.OnChange(_ => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.target.GetHandlerValue<PositionProperty>());
		}
	}
}
