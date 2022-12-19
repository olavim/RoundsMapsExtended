using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.MapObjects
{
	/// <summary>
	/// Spatial map objects represent map objects that are described with position, scale and rotation.
	/// Typical spatial map objects are, for example, boxes and other obstacles.
	/// </summary>
	public abstract class SpatialMapObject : MapObject, IMapObjectAnimation, IMapObjectPosition, IMapObjectScale, IMapObjectRotation
	{
		private readonly List<IAnimationComponent> animationComponents;

		public Vector3 position { get; set; } = Vector3.zero;
		public Vector3 scale { get; set; } = Vector3.one * 2;
		public Quaternion rotation { get; set; } = Quaternion.identity;
		public List<AnimationKeyframe> keyframes { get; set; } = new List<AnimationKeyframe>();

		public SpatialMapObject()
		{
			this.animationComponents = new List<IAnimationComponent>()
			{
				new PositionComponent(this),
				new ScaleComponent(this),
				new RotationComponent(this)
			};
		}

		public List<IAnimationComponent> GetAnimationComponents() => this.animationComponents;

		public override string ToString()
		{
			return $"{base.ToString()}\nposition: {this.position}\nsize: {this.scale}\nrotation: {this.rotation.eulerAngles.z}\nkeyframes: {this.keyframes.Count}";
		}
	}

	public class SpatialMapObjectInstance : MonoBehaviour { }
}
