using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt
{
	public interface IAnimationComponentValue
	{
		object Value { get; set; }
		IAnimationComponentValue Clone();
		void SetValueFrom(MapObjectData data);
	}

	public interface IAnimationComponentValue<T> : IAnimationComponentValue
	{
		new T Value { get; set; }
		new IAnimationComponentValue<T> Clone();
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

		public abstract AnimationComponentValue<T> Clone();

		IAnimationComponentValue<T> IAnimationComponentValue<T>.Clone()
		{
			return this.Clone();
		}

		IAnimationComponentValue IAnimationComponentValue.Clone()
		{
			return this.Clone();
		}

		public abstract void SetValueFrom(MapObjectData data);
	}

	public class PositionComponentValue : AnimationComponentValue<Vector2>
	{
		public PositionComponentValue(Vector2 value) : base(value) { }
		public override AnimationComponentValue<Vector2> Clone() => new PositionComponentValue(this.Value);

		public override void SetValueFrom(MapObjectData data)
		{
			this.Value = ((IMapObjectPosition) data).position;
		}
	}

	public class ScaleComponentValue : AnimationComponentValue<Vector2>
	{
		public ScaleComponentValue(Vector2 value) : base(value) { }
		public override AnimationComponentValue<Vector2> Clone() => new ScaleComponentValue(this.Value);

		public override void SetValueFrom(MapObjectData data)
		{
			this.Value = ((IMapObjectScale) data).scale;
		}
	}

	public class RotationComponentValue : AnimationComponentValue<Quaternion>
	{
		public RotationComponentValue(Quaternion value) : base(value) { }
		public override AnimationComponentValue<Quaternion> Clone() => new RotationComponentValue(this.Value);

		public override void SetValueFrom(MapObjectData data)
		{
			this.Value = ((IMapObjectRotation) data).rotation;
		}
	}
}
