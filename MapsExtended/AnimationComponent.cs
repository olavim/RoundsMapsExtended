// using MapsExt.MapObjects.Properties;
// using UnityEngine;

// namespace MapsExt
// {
// 	public interface IAnimationComponent
// 	{
// 		IMapObjectProperty Value { get; set; }
// 		void Lerp(GameObject go, object start, object end, float value);
// 	}

// 	public interface IAnimationComponent<T> : IAnimationComponent where T : IMapObjectProperty
// 	{
// 		new T Value { get; set; }
// 		void Lerp(GameObject go, T start, T end, float value);
// 	}

// 	public abstract class AnimationComponent<T> : IAnimationComponent<T> where T : IMapObjectProperty
// 	{
// 		IMapObjectProperty IAnimationComponent.Value
// 		{
// 			get => this.Value;
// 			set => this.Value = (T) value;
// 		}
// 		public T Value { get; set; }

// 		public void Lerp(GameObject go, object start, object end, float value) => this.Lerp(go, (T) start, (T) end, value);
// 		public abstract void Lerp(GameObject go, T start, T end, float value);

// 		protected AnimationComponent(T property)
// 		{
// 			this.Value = property;
// 		}
// 	}

// 	public class PositionComponent : AnimationComponent<PositionProperty>
// 	{
// 		public PositionComponent(PositionProperty property) : base(property) { }

// 		public override void Lerp(GameObject go, PositionProperty start, PositionProperty end, float value)
// 		{
// 			go.transform.position = Vector2.Lerp(start, end, value);
// 		}
// 	}

// 	public class ScaleComponent : AnimationComponent<ScaleProperty>
// 	{
// 		public ScaleComponent(ScaleProperty property) : base(property) { }

// 		public override void Lerp(GameObject go, ScaleProperty start, ScaleProperty end, float value)
// 		{
// 			go.transform.localScale = Vector2.Lerp(start, end, value);
// 		}
// 	}

// 	public class RotationComponent : AnimationComponent<RotationProperty>
// 	{
// 		public RotationComponent(RotationProperty property) : base(property) { }

// 		public override void Lerp(GameObject go, RotationProperty start, RotationProperty end, float value)
// 		{
// 			go.transform.rotation = Quaternion.Lerp(start, end, value);
// 		}
// 	}
// }
