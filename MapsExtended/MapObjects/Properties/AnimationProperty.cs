using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IAnimated
	{
		AnimationProperty Animation { get; }
	}

	public class AnimationProperty : IProperty
	{
		public List<AnimationKeyframe> keyframes;

		public AnimationProperty(params ILinearProperty[] properties)
		{
			this.keyframes = new List<AnimationKeyframe>() { new AnimationKeyframe(properties) };
		}

		public AnimationProperty(IEnumerable<ILinearProperty> properties)
		{
			this.keyframes = new List<AnimationKeyframe>() { new AnimationKeyframe(properties) };
		}
	}

	[PropertySerializer]
	public class AnimationPropertySerializer : PropertySerializer<AnimationProperty>
	{
		public override void Serialize(GameObject instance, AnimationProperty property)
		{
			var anim = instance.GetComponent<MapObjectAnimation>();

			if (anim != null)
			{
				foreach (var frame in anim.keyframes.Skip(1))
				{
					property.keyframes.Add(new AnimationKeyframe(frame));
				}
			}
		}

		public override void Deserialize(AnimationProperty property, GameObject target)
		{
			if (property.keyframes.Count > 1)
			{
				var dataFrames = property.keyframes.ToList();
				dataFrames.Insert(0, new AnimationKeyframe(property.keyframes[0]));

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
				target.GetComponent<MapObjectAnimation>()?.keyframes.Insert(0, new AnimationKeyframe(property.keyframes[0]));
			}
		}
	}
}
