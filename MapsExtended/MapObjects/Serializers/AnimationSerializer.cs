using System.Collections.Generic;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public interface IMapObjectAnimation
	{
		List<AnimationKeyframe> keyframes { get; set; }
		List<IAnimationComponent> GetAnimationComponents();
	}

	[MapObjectSerializer]
	public class AnimationSerializer : IMapObjectSerializer<IMapObjectAnimation>
	{
		public virtual void Serialize(GameObject instance, IMapObjectAnimation target)
		{
			var anim = instance.GetComponent<MapObjectAnimation>();
			if (anim && anim.keyframes.Count > 0)
			{
				foreach (var frame in anim.keyframes.Skip(1))
				{
					target.keyframes.Add(new AnimationKeyframe(frame));
				}
			}
		}

		public virtual void Deserialize(IMapObjectAnimation data, GameObject target)
		{
			if (data.keyframes != null && data.keyframes.Count > 0)
			{
				var dataFrames = data.keyframes.ToList();
				dataFrames.Insert(0, new AnimationKeyframe(data));

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
				target.GetComponent<MapObjectAnimation>()?.keyframes.Insert(0, new AnimationKeyframe(data));
			}
		}
	}
}
