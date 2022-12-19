using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt
{
	public interface IAnimationComponent
	{
		IAnimationComponentValue Value { get; }
		void Lerp(object start, object end, float value);
	}

	public interface IAnimationComponent<T> : IAnimationComponent
	{
		new IAnimationComponentValue<T> Value { get; }
		void Lerp(T start, T end, float value);
	}

	public abstract class AnimationComponent<T> : IAnimationComponent<T>
	{
		IAnimationComponentValue IAnimationComponent.Value => this.Value;
		public abstract IAnimationComponentValue<T> Value { get; }

		public void Lerp(object start, object end, float value) => this.Lerp((T) start, (T) end, value);
		public abstract void Lerp(T start, T end, float value);
	}

	public class PositionComponent : AnimationComponent<Vector2>
	{
		private IMapObjectPosition mapObject;
		public override IAnimationComponentValue<Vector2> Value => new PositionComponentValue(this.mapObject.position);

		public PositionComponent(IMapObjectPosition mapObject)
		{
			this.mapObject = mapObject;
		}

		public override void Lerp(Vector2 start, Vector2 end, float value)
		{
			this.mapObject.position = Vector2.Lerp(start, end, value);
		}
	}

	public class ScaleComponent : AnimationComponent<Vector2>
	{
		private IMapObjectScale mapObject;
		public override IAnimationComponentValue<Vector2> Value => new ScaleComponentValue(this.mapObject.scale);

		public ScaleComponent(IMapObjectScale mapObject)
		{
			this.mapObject = mapObject;
		}

		public override void Lerp(Vector2 start, Vector2 end, float value)
		{
			this.mapObject.scale = Vector2.Lerp(start, end, value);
		}
	}

	public class RotationComponent : AnimationComponent<Quaternion>
	{
		private IMapObjectRotation mapObject;
		public override IAnimationComponentValue<Quaternion> Value => new RotationComponentValue(this.mapObject.rotation);

		public RotationComponent(IMapObjectRotation mapObject)
		{
			this.mapObject = mapObject;
		}

		public override void Lerp(Quaternion start, Quaternion end, float value)
		{
			this.mapObject.rotation = Quaternion.Lerp(start, end, value);
		}
	}
}
