using MapsExt.Editor.UI;
using MapsExt.Properties;
using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Editor.Properties
{
	[EditorPropertySerializer(typeof(AnimationProperty))]
	public class EditorAnimationPropertySerializer : IPropertySerializer<AnimationProperty>
	{
		public virtual AnimationProperty ReadProperty(GameObject instance)
		{
			var keyframes = new List<AnimationKeyframe>();
			var anim = instance.GetComponent<MapObjectAnimation>();

			if (anim != null)
			{
				keyframes.AddRange(anim.Keyframes);
				if (keyframes.Count > 0)
				{
					keyframes[0] = new AnimationKeyframe(instance.ReadProperties<ILinearProperty>());
				}
			}

			return new AnimationProperty(keyframes);
		}

		public virtual void WriteProperty(AnimationProperty property, GameObject target)
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

	[InspectorElement(typeof(AnimationProperty))]
	public class AnimationButtonElement : ButtonElement
	{
		public override void OnUpdate()
			=> this.ButtonText = this.Context.Editor.AnimationHandler.Animation ? "Close Animation" : "Edit Animation";

		protected override void OnClick()
			=> this.Context.Editor.AnimationHandler.ToggleAnimation(this.Context.InspectorTarget);
	}
}
