using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.Properties
{
	public class AnimationProperty : IProperty
	{
		private readonly AnimationKeyframe[] _keyframes;

		public AnimationKeyframe[] Keyframes => this._keyframes;

		public AnimationProperty()
		{
			this._keyframes = new AnimationKeyframe[] { };
		}

		public AnimationProperty(params ILinearProperty[] properties)
		{
			this._keyframes = new[] { new AnimationKeyframe(properties) };
		}

		public AnimationProperty(IEnumerable<ILinearProperty> properties)
		{
			this._keyframes = new[] { new AnimationKeyframe(properties) };
		}

		public AnimationProperty(IEnumerable<AnimationKeyframe> keyframes)
		{
			this._keyframes = keyframes.ToArray();
		}
	}

	[PropertySerializer(typeof(AnimationProperty))]
	public class AnimationPropertySerializer : PropertySerializer<AnimationProperty>
	{
		public override AnimationProperty Serialize(GameObject instance)
		{
			var keyframes = new List<AnimationKeyframe>();
			var anim = instance.GetComponent<MapObjectAnimation>();

			if (anim != null)
			{
				keyframes.AddRange(anim.Keyframes);
			}

			return new AnimationProperty(keyframes);
		}

		public override void Deserialize(AnimationProperty property, GameObject target)
		{
			if (property.Keyframes.Length > 1)
			{
				var keyframes = property.Keyframes.ToList();
				keyframes.ForEach(k => k.UpdateCurve());
				target.GetOrAddComponent<MapObjectAnimation>().Keyframes = keyframes;
			}
			else if (target.GetComponent<MapObjectAnimation>())
			{
				GameObject.Destroy(target.GetComponent<MapObjectAnimation>());
			}
		}
	}
}
