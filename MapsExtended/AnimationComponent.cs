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
		public IAnimationComponentValue<T> Value { get; protected set; }

		public void Lerp(GameObject go, object start, object end, float value) => this.Lerp(go, (T) start, (T) end, value);
		public abstract void Lerp(GameObject go, T start, T end, float value);

		public void Lerp(MapObjectData data, object start, object end, float value) => this.Lerp(data, (T) start, (T) end, value);
		public abstract void Lerp(MapObjectData data, T start, T end, float value);
	}

	public class PositionComponent : AnimationComponent<Vector2>
	{
		public PositionComponent(IMapObjectPosition mapObject)
		{
			this.Value = new PositionComponentValue(mapObject.position);
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
		public ScaleComponent(IMapObjectScale mapObject)
		{
			this.Value = new ScaleComponentValue(mapObject.scale);
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
		public RotationComponent(IMapObjectRotation mapObject)
		{
			this.Value = new RotationComponentValue(mapObject.rotation);
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
