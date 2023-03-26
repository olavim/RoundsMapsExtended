using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	/// <summary>
	/// Spatial map objects represent map objects that are described with position, scale and rotation.
	/// Typical spatial map objects are, for example, boxes and other obstacles.
	/// </summary>
	public abstract class SpatialMapObjectData : MapObjectData, IAnimated
	{
		public PositionProperty Position { get; set; } = new PositionProperty();
		public ScaleProperty Scale { get; set; } = new ScaleProperty();
		public RotationProperty Rotation { get; set; } = new RotationProperty();
		public AnimationProperty Animation { get; set; }

		protected SpatialMapObjectData()
		{
			this.Animation = new AnimationProperty(this.Position, this.Scale, this.Rotation);
		}
	}

	public class SpatialMapObjectInstance : MonoBehaviour { }
}
