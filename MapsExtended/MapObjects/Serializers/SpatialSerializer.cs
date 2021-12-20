using UnityEngine;
using UnboundLib;
using System.Collections.Generic;
using System.Linq;

namespace MapsExt.MapObjects
{
	public class SpatialMapObject : MapObject
	{
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one * 2;
		public Quaternion rotation = Quaternion.identity;
		public List<AnimationKeyframe> animationKeyframes = new List<AnimationKeyframe>();
	}

	/// <summary>
	///	Spatial map objects represent map objects that are described with position, scale and rotation.
	///	Typical spatial map objects are, for example, boxes and obstacles.
	/// </summary>
	public static class SpatialSerializer
	{
		public static void Serialize(GameObject instance, SpatialMapObject target)
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

		public static void Deserialize(SpatialMapObject data, GameObject target)
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

	public class SpatialMapObjectInstance : MonoBehaviour { }
}
