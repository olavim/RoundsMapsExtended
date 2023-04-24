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
		[SerializeField] private PositionProperty _position = new();
		[SerializeField] private ScaleProperty _scale = new();
		[SerializeField] private RotationProperty _rotation = new();
		[SerializeField] private AnimationKeyframe[] _keyframes = new AnimationKeyframe[0];

		public PositionProperty Position { get => this._position; set => this._position = value; }
		public ScaleProperty Scale { get => this._scale; set => this._scale = value; }
		public RotationProperty Rotation { get => this._rotation; set => this._rotation = value; }
		public AnimationProperty Animation
		{
			get => new(this._keyframes);
			set => this._keyframes = value.Keyframes;
		}
	}
}
