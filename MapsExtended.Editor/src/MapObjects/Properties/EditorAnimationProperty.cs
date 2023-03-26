using MapsExt.Editor.UI;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnityEngine;
using UnityEngine.UI;

namespace MapsExt.Editor.MapObjects.Properties
{
	[EditorMapObjectPropertySerializer]
	public class EditorAnimationPropertySerializer : AnimationPropertySerializer, IInspectable
	{
		public override void Deserialize(AnimationProperty property, GameObject target)
		{
			base.Deserialize(property, target);

			var anim = target.GetComponent<MapObjectAnimation>();
			if (anim)
			{
				anim.playOnAwake = false;
			}
		}

		public virtual void OnInspectorLayout(MapObjectInspector inspector, InspectorLayoutBuilder builder)
		{
			builder.Button()
				.ClickEvent(() => inspector.editor.animationHandler.ToggleAnimation(inspector.target.gameObject))
				.UpdateEvent(button => button.GetComponentInChildren<Text>().text = inspector.editor.animationHandler.animation ? "Close Animation" : "Edit Animation");
		}
	}
}
