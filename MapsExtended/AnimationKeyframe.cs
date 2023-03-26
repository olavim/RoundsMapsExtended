using MapsExt.MapObjects.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapsExt
{
	public class AnimationKeyframe
	{
		public List<ILinearProperty> componentValues;
		public float duration;
		public CurveType curveType;

		[NonSerialized]
		public BezierAnimationCurve curve;

		public AnimationKeyframe()
		{
			this.componentValues = new List<ILinearProperty>();
			this.duration = 1;
			this.curveType = CurveType.Linear;

			this.UpdateCurve();
		}

		public AnimationKeyframe(IEnumerable<ILinearProperty> values)
		{
			this.componentValues = values.ToList();
			this.duration = 1;
			this.curveType = CurveType.Linear;

			this.UpdateCurve();
		}

		public AnimationKeyframe(AnimationKeyframe frame)
		{
			this.componentValues = frame.componentValues.ToList();
			this.duration = frame.duration;
			this.curveType = frame.curveType;

			this.UpdateCurve();
		}

		public void UpdateCurve()
		{
			this.curve = this.GetCurve();
		}

		public T GetComponentValue<T>() where T : IMapObjectProperty
		{
			return (T) this.componentValues.Find(v => typeof(T).IsAssignableFrom(v.GetType()));
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
