using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt
{
	public class AnimationKeyframe
	{
		public Vector3 position;
		public Vector3 scale;
		public Quaternion rotation;

		public AnimationCurve curve = new AnimationCurve(new Keyframe[] { new Keyframe(0, 0), new Keyframe(0, 1) });
		public float animationSpeed = 1;

		public AnimationKeyframe()
		{
			this.position = Vector3.zero;
			this.scale = Vector3.one;
			this.rotation = Quaternion.identity;
		}

		public AnimationKeyframe(SpatialMapObject mapObject)
		{
			this.position = mapObject.position;
			this.scale = mapObject.scale;
			this.rotation = mapObject.rotation;
		}

		public AnimationKeyframe(AnimationKeyframe frame)
		{
			this.position = frame.position;
			this.scale = frame.scale;
			this.rotation = frame.rotation;
			this.animationSpeed = frame.animationSpeed;
		}
	}
}
