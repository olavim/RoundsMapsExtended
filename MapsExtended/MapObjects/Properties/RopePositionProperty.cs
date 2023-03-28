using UnityEngine;
using System;

namespace MapsExt.MapObjects.Properties
{
	public class RopePositionProperty : IProperty, IEquatable<RopePositionProperty>
	{
		public Vector2 startPosition;
		public Vector2 endPosition;

		public RopePositionProperty() : this(Vector2.up, Vector2.down) { }

		public RopePositionProperty(Vector2 startPosition, Vector2 endPosition)
		{
			this.startPosition = startPosition;
			this.endPosition = endPosition;
		}

		public bool Equals(RopePositionProperty other) =>
			this.startPosition.Equals(other.startPosition) && this.endPosition.Equals(other.endPosition);
		public override bool Equals(object other) => other is RopePositionProperty prop && this.Equals(prop);
		public override int GetHashCode() => (this.startPosition, this.endPosition).GetHashCode();

		public static bool operator ==(RopePositionProperty a, RopePositionProperty b) => a.Equals(b);
		public static bool operator !=(RopePositionProperty a, RopePositionProperty b) => !a.Equals(b);
	}
}
