using MapsExt.MapObjects;
using UnityEngine;
using System;

namespace MapsExt
{
	public class AnimationKeyframe
	{
		public Vector3 position;
		public Vector3 scale;
		public Quaternion rotation;
		public float duration;
		public CurveType curveType;

		[NonSerialized]
		public BezierAnimationCurve curve;

		public AnimationKeyframe()
		{
			this.position = Vector3.zero;
			this.scale = Vector3.one;
			this.rotation = Quaternion.identity;
			this.duration = 1;
			this.curveType = CurveType.Linear;

			this.UpdateCurve();
		}

		public AnimationKeyframe(SpatialMapObject mapObject)
		{
			this.position = mapObject.position;
			this.scale = mapObject.scale;
			this.rotation = mapObject.rotation;
			this.duration = 1;
			this.curveType = CurveType.Linear;

			this.UpdateCurve();
		}

		public AnimationKeyframe(AnimationKeyframe frame)
		{
			this.position = frame.position;
			this.scale = frame.scale;
			this.rotation = frame.rotation;
			this.duration = frame.duration;
			this.curveType = frame.curveType;

			this.UpdateCurve();
		}

		public void UpdateCurve()
		{
			this.curve = this.GetCurve();
		}

		private BezierAnimationCurve GetCurve()
		{
			switch (this.curveType)
			{
				case CurveType.Linear:
					return new BezierAnimationCurve(0, 0, 1, 1);
				case CurveType.EaseIn:
					return new BezierAnimationCurve(0.12f, 0, 0.39f, 0);
				case CurveType.EaseOut:
					return new BezierAnimationCurve(0.61f, 1, 0.88f, 1);
				case CurveType.EaseInOut:
					return new BezierAnimationCurve(0.37f, 0, 0.63f, 1);
				default:
					return null;
			}
		}

		public enum CurveType
		{
			Linear,
			EaseIn,
			EaseOut,
			EaseInOut
		}
	}
}
