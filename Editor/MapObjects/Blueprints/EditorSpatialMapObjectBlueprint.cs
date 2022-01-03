using UnityEngine;
using MapsExt.MapObjects;
using MapsExt.Editor.ActionHandlers;
using UnboundLib;
using MapsExt.Editor.UI;
using MapsExt.Editor.Commands;
using UnityEngine.UI;

namespace MapsExt.Editor.MapObjects
{
	public abstract class EditorSpatialMapObjectBlueprint<T> : BaseEditorMapObjectBlueprint<T>, IInspectable where T : SpatialMapObject
	{
		public override void Deserialize(T data, GameObject target)
		{
			this.baseBlueprint.Deserialize(data, target);
			target.GetOrAddComponent<SpatialActionHandler>();
		}

		public override void Serialize(GameObject instance, T target) => this.baseBlueprint.Serialize(instance, target);

		public virtual void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Property<Vector2>("Position")
				.CommandGetter(value => new MoveCommand(inspector.targetHandler, inspector.targetHandler.transform.position, value))
				.ValueGetter(() => inspector.targetHandler.transform.position)
				.ChangeEvent(() => inspector.editor.UpdateRopeAttachments());

			builder.Property<Vector2>("Size")
				.CommandGetter(value => new ResizeCommand(inspector.targetHandler, inspector.targetHandler.transform.localScale, value))
				.ValueGetter(() => inspector.targetHandler.transform.localScale)
				.ChangeEvent(() => inspector.editor.UpdateRopeAttachments());

			builder.Property<Quaternion>("Rotation")
				.CommandGetter(value => new RotateCommand(inspector.targetHandler, inspector.targetHandler.transform.rotation, value))
				.ValueGetter(() => inspector.targetHandler.transform.rotation)
				.ChangeEvent(() => inspector.editor.UpdateRopeAttachments());

			builder.Divider();

			builder.Button()
				.ClickEvent(() => inspector.editor.animationHandler.ToggleAnimation(inspector.target.gameObject))
				.UpdateEvent(button => button.GetComponentInChildren<Text>().text = inspector.editor.animationHandler.animation ? "Close Animation" : "Edit Animation");
		}
	}
}
