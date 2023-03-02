using MapsExt.Editor.UI;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorMapObjectProperty]
	public class EditorPositionProperty : PositionProperty, IInspectable
	{
		public override void Deserialize(IMapObjectPosition data, GameObject target)
		{
			base.Deserialize(data, target);
			target.GetOrAddComponent<ActionHandlers.PositionHandler>();
			target.GetOrAddComponent<ActionHandlers.SelectionHandler>();
		}

		public void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Position")
				.ValueSetter(value => inspector.target.GetComponent<ActionHandlers.PositionHandler>().SetPosition(value))
				.OnChange(_ => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.target.transform.position);
		}
	}
}
