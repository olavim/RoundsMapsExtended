using UnityEngine;
using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using UnboundLib;
using MapsExt.Editor.UI;
using UnityEngine.UI;
using System.Collections;

namespace MapsExt.Editor.MapObjects
{
	public abstract class EditorSpatialMapObjectBlueprint<T> : BaseEditorMapObjectBlueprint<T>, IInspectable where T : SpatialMapObject
	{
		public override void Deserialize(T data, GameObject target)
		{
			this.baseBlueprint.Deserialize(data, target);

			var anim = target.GetComponent<MapObjectAnimation>();
			if (anim)
			{
				anim.playOnAwake = false;
			}

			target.GetOrAddComponent<PositionHandler>();
			target.GetOrAddComponent<SizeHandler>();
			target.GetOrAddComponent<ActionHandlers.RotationHandler>();
		}

		public override void Serialize(GameObject instance, T target) => this.baseBlueprint.Serialize(instance, target);

		public virtual void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Position")
				.ValueSetter(value => inspector.selectedObject.GetComponent<PositionHandler>().SetPosition(value))
				.OnChange(value => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.selectedObject.transform.position);

			builder.Property<Vector2>("Size")
				.ValueSetter(value => inspector.selectedObject.GetComponent<SizeHandler>().SetSize(value))
				.OnChange(value => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.selectedObject.transform.localScale);

			builder.Property<Quaternion>("Rotation")
				.ValueSetter(value => inspector.selectedObject.GetComponent<ActionHandlers.RotationHandler>().SetRotation(value))
				.OnChange(value => inspector.editor.UpdateRopeAttachments())
				.ValueGetter(() => inspector.selectedObject.transform.rotation);

			builder.Divider();

			builder.Button()
				.ClickEvent(() => inspector.editor.animationHandler.ToggleAnimation(inspector.target.gameObject))
				.UpdateEvent(button => button.GetComponentInChildren<Text>().text = inspector.editor.animationHandler.animation ? "Close Animation" : "Edit Animation");
		}
	}
}
