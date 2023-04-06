using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	/// <summary>
	/// Spatial map objects represent map objects that are described with position, scale and rotation.
	/// Typical spatial map objects are, for example, boxes and other obstacles.
	/// </summary>
	public abstract class SpatialMapObjectData : MapObjectData
	{
		private PositionProperty _position;
		private ScaleProperty _scale;
		private RotationProperty _rotation;
		private AnimationKeyframe[] _keyframes;

		public PositionProperty Position { get => this._position; set => this._position = value; }
		public ScaleProperty Scale { get => this._scale; set => this._scale = value; }
		public RotationProperty Rotation { get => this._rotation; set => this._rotation = value; }
		public AnimationProperty Animation
		{
			get => new(this._keyframes);
			set => this._keyframes = value.Keyframes;
		}

		protected SpatialMapObjectData()
		{
			this.Position = new();
			this.Scale = new();
			this.Rotation = new();
			this.Animation = new();
		}
	}
}
