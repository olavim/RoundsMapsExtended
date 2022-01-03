using System.Linq;
using UnboundLib;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public abstract class SpatialMapObjectBlueprint<T> : BaseMapObjectBlueprint<T> where T : SpatialMapObject
	{
		public override void Serialize(GameObject instance, T target)
		{
			target.position = instance.transform.position;
			target.scale = instance.transform.localScale;
			target.rotation = instance.transform.rotation;

			var anim = instance.GetComponent<MapObjectAnimation>();
			if (anim && anim.keyframes.Count > 0)
			{
				foreach (var frame in anim.keyframes.Skip(1))
				{
					target.animationKeyframes.Add(new AnimationKeyframe(frame));
				}
			}
		}

		public override void Deserialize(T data, GameObject target)
		{
			/* SpatialMapObjectInstance doesn't add any functionality, but it offers a convenient way
			 * to find "spatial" map objects from scene.
			 */
			target.GetOrAddComponent<SpatialMapObjectInstance>();

			target.transform.position = data.position;
			target.transform.localScale = data.scale;
			target.transform.rotation = data.rotation;

			if (data.animationKeyframes != null && data.animationKeyframes.Count > 0)
			{
				var dataFrames = data.animationKeyframes.ToList();
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
