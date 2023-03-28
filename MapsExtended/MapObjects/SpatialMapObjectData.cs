using MapsExt.MapObjects.Properties;

namespace MapsExt.MapObjects
{
	/// <summary>
	/// Spatial map objects represent map objects that are described with position, scale and rotation.
	/// Typical spatial map objects are, for example, boxes and other obstacles.
	/// </summary>
	public abstract class SpatialMapObjectData : MapObjectData, IAnimated
	{
		public PositionProperty position = new PositionProperty();
		public ScaleProperty scale = new ScaleProperty();
		public RotationProperty rotation = new RotationProperty();
		public AnimationProperty animation;

		public AnimationProperty Animation => this.animation;

		protected SpatialMapObjectData()
		{
			this.animation = new AnimationProperty(this.position, this.scale, this.rotation);
		}
	}
}
