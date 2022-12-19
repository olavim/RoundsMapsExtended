using UnityEngine;

namespace MapsExt
{
	public interface IAnimationComponentValue
	{
		object Value { get; set; }
	}

	public interface IAnimationComponentValue<T> : IAnimationComponentValue
	{
		new T Value { get; set; }
	}

	public abstract class AnimationComponentValue<T> : IAnimationComponentValue<T>
	{
		object IAnimationComponentValue.Value
		{
			get => this.Value;
			set => this.Value = (T) value;
		}

		public virtual T Value { get; set; } = default;

		public AnimationComponentValue(T value)
		{
			this.Value = value;
		}
	}

	public class PositionComponentValue : AnimationComponentValue<Vector2>
	{
		public PositionComponentValue(Vector2 value) : base(value) { }
	}

	public class ScaleComponentValue : AnimationComponentValue<Vector2>
	{
		public ScaleComponentValue(Vector2 value) : base(value) { }
	}

	public class RotationComponentValue : AnimationComponentValue<Quaternion>
	{
		public RotationComponentValue(Quaternion value) : base(value) { }
	}
}
