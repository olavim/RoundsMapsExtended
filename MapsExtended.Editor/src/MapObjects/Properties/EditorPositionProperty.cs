using MapsExt.Editor.UI;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorMapObjectPropertySerializer]
	public class EditorPositionPropertySerializer : PositionPropertySerializer, IInspectable
	{
		public override void Deserialize(PositionProperty property, GameObject target)
		{
			base.Deserialize(property, target);
			target.GetOrAddComponent<ActionHandlers.PositionHandler>();
			target.GetOrAddComponent<ActionHandlers.SelectionHandler>();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Position")
				.ValueSetter(value => inspector.target.GetComponent<ActionHandlers.PositionHandler>().SetValue(value))
				.OnChange(_ => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.target.transform.position);
		}
	}
}
