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

		public AnimationProperty(params AnimationKeyframe[] keyframes)
		{
			this._keyframes = keyframes.ToArray();
		}
	}

	[PropertySerializer(typeof(AnimationProperty))]
	public class AnimationPropertySerializer : IPropertyWriter<AnimationProperty>
	{
		public virtual void WriteProperty(AnimationProperty property, GameObject target)
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
