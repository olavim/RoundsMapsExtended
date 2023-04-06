using MapsExt.Editor.UI;
using MapsExt.Properties;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(AnimationProperty))]
	public class EditorAnimationPropertySerializer : AnimationPropertySerializer
	{
		public override void Deserialize(AnimationProperty property, GameObject target)
		{
			if (property.Keyframes.Length > 0)
			{
				var anim = target.GetOrAddComponent<MapObjectAnimation>();
				anim.PlayOnAwake = false;
				anim.Stop();
				anim.Keyframes = property.Keyframes.ToList();
			}
			else if (target.GetComponent<MapObjectAnimation>())
			{
				GameObject.Destroy(target.GetComponent<MapObjectAnimation>());
			}
		}
	}

	[PropertyInspector(typeof(AnimationProperty))]
	public class AnimationButtonElement : ButtonElement
	{
		public override void OnUpdate()
			=> this.ButtonText = this.Context.Editor.AnimationHandler.Animation ? "Close Animation" : "Edit Animation";

		protected override void OnClick()
			=> this.Context.Editor.AnimationHandler.ToggleAnimation(this.Context.InspectorTarget);
	}
}
