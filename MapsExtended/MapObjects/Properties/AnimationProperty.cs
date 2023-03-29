using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects.Properties
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

	[PropertySerializer]
	public class AnimationPropertySerializer : PropertySerializer<AnimationProperty>
	{
		public override AnimationProperty Serialize(GameObject instance)
		{
			var animInstance = instance.GetComponent<MapObjectAnimationInstance>();
			var keyframes = animInstance.keyframes.Take(1).ToList();

			var anim = instance.GetComponent<MapObjectAnimation>();

			if (anim != null)
			{
				foreach (var frame in anim.keyframes.Skip(1))
				{
					keyframes.Add(new AnimationKeyframe(frame));
				}
			}

			return new AnimationProperty(keyframes);
		}

		public override void Deserialize(AnimationProperty property, GameObject target)
		{
			var instance = target.GetOrAddComponent<MapObjectAnimationInstance>();
			instance.keyframes = property.Keyframes.Take(1).ToArray();

			if (property.Keyframes.Length > 1)
			{
				var dataFrames = property.Keyframes.ToList();
				dataFrames.Insert(0, new AnimationKeyframe(property.Keyframes[0]));

				var anim = target.GetOrAddComponent<MapObjectAnimation>();

				for (int i = 0; i < dataFrames.Count; i++)
				{
					dataFrames[i].UpdateCurve();

					if (i < anim.keyframes.Count)
					{
						anim.keyframes[i] = dataFrames[i];
					}
					else
					{
						anim.keyframes.Add(dataFrames[i]);
					}
				}

				anim.keyframes = anim.keyframes.Take(dataFrames.Count).ToList();
			}
			else
			{
				target.GetComponent<MapObjectAnimation>()?.keyframes.Clear();
				target.GetComponent<MapObjectAnimation>()?.keyframes.Insert(0, new AnimationKeyframe(property.Keyframes[0]));
			}
		}

		class MapObjectAnimationInstance : MonoBehaviour
		{
			public AnimationKeyframe[] keyframes;
		}
	}
}
