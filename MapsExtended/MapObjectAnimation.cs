using MapsExt.MapObjects;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt
{
	public class MapObjectAnimation : MonoBehaviour
	{
		public List<AnimationKeyframe> keyframes;

		public void Awake()
		{
			this.keyframes = new List<AnimationKeyframe>();
		}

		public void Initialize(SpatialMapObject mapObject)
		{
			this.keyframes.Clear();
			this.keyframes.Add(new AnimationKeyframe(mapObject));
		}

		public void AddKeyframe()
		{
			this.keyframes.Add(new AnimationKeyframe(this.keyframes[this.keyframes.Count - 1]));
		}
	}
}
