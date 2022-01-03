using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.MapObjects
{
	/// <summary>
	///	Spatial map objects represent map objects that are described with position, scale and rotation.
	///	Typical spatial map objects are, for example, boxes and other obstacles.
	/// </summary>
	public abstract class SpatialMapObject : MapObject
	{
		public Vector3 position = Vector3.zero;
		public Vector3 scale = Vector3.one * 2;
		public Quaternion rotation = Quaternion.identity;
		public List<AnimationKeyframe> animationKeyframes = new List<AnimationKeyframe>();

		public override string ToString()
		{
			return $"{base.ToString()}\nposition: {this.position}\nsize: {this.scale}\nrotation: {this.rotation.eulerAngles.z}\nkeyframes: {this.animationKeyframes.Count}";
		}
	}

	public class SpatialMapObjectInstance : MonoBehaviour { }
}
