using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects.Properties
{
	public interface IAnimated
	{
		AnimationProperty Animation { get; set; }
	}

	public class AnimationProperty : IMapObjectProperty
	{
		public List<AnimationKeyframe> Keyframes { get; set; }

		public AnimationProperty(params ILinearProperty[] properties)
		{
			this.Keyframes = new List<AnimationKeyframe>() { new AnimationKeyframe(properties) };
		}

		public AnimationProperty(IEnumerable<ILinearProperty> properties)
		{
			this.Keyframes = new List<AnimationKeyframe>() { new AnimationKeyframe(properties) };
		}
	}

	[MapObjectPropertySerializer]
	public class AnimationPropertySerializer : MapObjectPropertySerializer<AnimationProperty>
	{
		public override void Serialize(GameObject instance, AnimationProperty property)
		{
			var anim = instance.GetComponent<MapObjectAnimation>();

			if (anim != null)
			{
				foreach (var frame in anim.keyframes.Skip(1))
				{
					property.Keyframes.Add(new AnimationKeyframe(frame));
				}
			}
		}

		public override void Deserialize(AnimationProperty property, GameObject target)
		{
			if (property.Keyframes.Count > 1)
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
	}
}
