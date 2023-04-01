using MapsExt.MapObjects.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MapsExt
{
	public enum CurveType
	{
		Linear,
		EaseIn,
		EaseOut,
		EaseInOut
	}

	public class AnimationKeyframe
	{
		private List<ILinearProperty> _componentValues;
		private float _duration;
		private CurveType _curveType;

		[NonSerialized]
		private BezierAnimationCurve _curve;

		public List<ILinearProperty> ComponentValues { get => this._componentValues; set => this._componentValues = value; }
		public float Duration { get => this._duration; set => this._duration = value; }
		public CurveType CurveType { get => this._curveType; set => this._curveType = value; }
		public BezierAnimationCurve Curve { get => this._curve; set => this._curve = value; }

		public AnimationKeyframe()
		{
			this.ComponentValues = new();
			this.Duration = 1;
			this.CurveType = CurveType.Linear;

			this.UpdateCurve();
		}

		public AnimationKeyframe(params ILinearProperty[] values) : this((IEnumerable<ILinearProperty>) values) { }

		public AnimationKeyframe(IEnumerable<ILinearProperty> values)
		{
			this.ComponentValues = values.ToList();
			this.Duration = 1;
			this.CurveType = CurveType.Linear;

			this.UpdateCurve();
		}

		public AnimationKeyframe(AnimationKeyframe frame)
		{
			this.ComponentValues = frame.ComponentValues.ToList();
			this.Duration = frame.Duration;
			this.CurveType = frame.CurveType;

			this.UpdateCurve();
		}

		public void UpdateCurve()
		{
			this.Curve = this.GetCurve();
		}

		public T GetComponentValue<T>() where T : IProperty
		{
			return (T) this.ComponentValues.Find(v => typeof(T).IsAssignableFrom(v.GetType()));
		}

		private BezierAnimationCurve GetCurve()
		{
			return this.CurveType switch
			{
				CurveType.Linear => new(0, 0, 1, 1),
				CurveType.EaseIn => new(0.12f, 0, 0.39f, 0),
				CurveType.EaseOut => new(0.61f, 1, 0.88f, 1),
				CurveType.EaseInOut => new(0.37f, 0, 0.63f, 1),
				_ => null,
			};
		}
	}
}
