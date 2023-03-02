using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt
{
	public interface IAnimationComponent
	{
		IAnimationComponentValue Value { get; }
		void Lerp(GameObject go, object start, object end, float value);
		void Lerp(MapObjectData data, object start, object end, float value);
	}

	public interface IAnimationComponent<T> : IAnimationComponent
	{
		new IAnimationComponentValue<T> Value { get; }
		void Lerp(GameObject go, T start, T end, float value);
		void Lerp(MapObjectData data, T start, T end, float value);
	}

	public abstract class AnimationComponent<T> : IAnimationComponent<T>
	{
		IAnimationComponentValue IAnimationComponent.Value => this.Value;
		public abstract IAnimationComponentValue<T> Value { get; }

		public void Lerp(GameObject go, object start, object end, float value) => this.Lerp(go, (T) start, (T) end, value);
		public abstract void Lerp(GameObject go, T start, T end, float value);

		public void Lerp(MapObjectData data, object start, object end, float value) => this.Lerp(data, (T) start, (T) end, value);
		public abstract void Lerp(MapObjectData data, T start, T end, float value);
	}

	public class PositionComponent : AnimationComponent<Vector2>
	{
		public readonly IMapObjectPosition mapObject;

		public override IAnimationComponentValue<Vector2> Value => new PositionComponentValue(this.mapObject.position);

		public PositionComponent(IMapObjectPosition mapObject)
		{
			this.mapObject = mapObject;
		}

		public override void Lerp(GameObject go, Vector2 start, Vector2 end, float value)
		{
			go.transform.position = Vector2.Lerp(start, end, value);
		}

		public override void Lerp(MapObjectData data, Vector2 start, Vector2 end, float value)
		{
			((IMapObjectPosition) data).position = Vector2.Lerp(start, end, value);
		}
	}

	public class ScaleComponent : AnimationComponent<Vector2>
	{
		public readonly IMapObjectScale mapObject;

		public override IAnimationComponentValue<Vector2> Value => new ScaleComponentValue(this.mapObject.scale);

		public ScaleComponent(IMapObjectScale mapObject)
		{
			this.mapObject = mapObject;
		}

		public override void Lerp(GameObject go, Vector2 start, Vector2 end, float value)
		{
			go.transform.localScale = Vector2.Lerp(start, end, value);
		}

		public override void Lerp(MapObjectData data, Vector2 start, Vector2 end, float value)
		{
			((IMapObjectScale) data).scale = Vector2.Lerp(start, end, value);
		}
	}

	public class RotationComponent : AnimationComponent<Quaternion>
	{
		public readonly IMapObjectRotation mapObject;

		public override IAnimationComponentValue<Quaternion> Value => new RotationComponentValue(this.mapObject.rotation);

		public RotationComponent(IMapObjectRotation mapObject)
		{
			this.mapObject = mapObject;
		}

		public override void Lerp(GameObject go, Quaternion start, Quaternion end, float value)
		{
			go.transform.rotation = Quaternion.Lerp(start, end, value);
		}

		public override void Lerp(MapObjectData data, Quaternion start, Quaternion end, float value)
		{
			((IMapObjectRotation) data).rotation = Quaternion.Lerp(start, end, value);
		}
	}
}
