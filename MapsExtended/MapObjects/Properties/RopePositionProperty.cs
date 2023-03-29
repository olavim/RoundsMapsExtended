using UnityEngine;
using System;
using UnboundLib;

namespace MapsExt.MapObjects.Properties
{
	public class RopePositionProperty : IProperty, IEquatable<RopePositionProperty>
	{
		private Vector2 _pos1;
		private Vector2 _pos2;

		public Vector2 StartPosition { get => this._pos1; set => this._pos1 = value; }
		public Vector2 EndPosition { get => this._pos2; set => this._pos2 = value; }

		public RopePositionProperty() : this(Vector2.up, Vector2.down) { }

		public RopePositionProperty(Vector2 startPosition, Vector2 endPosition)
		{
			this.StartPosition = startPosition;
			this.EndPosition = endPosition;
		}

		public bool Equals(RopePositionProperty other) =>
			this.StartPosition.Equals(other.StartPosition) && this.EndPosition.Equals(other.EndPosition);
		public override bool Equals(object other) => other is RopePositionProperty prop && this.Equals(prop);
		public override int GetHashCode() => (this.StartPosition, this.EndPosition).GetHashCode();

		public static bool operator ==(RopePositionProperty a, RopePositionProperty b) => a.Equals(b);
		public static bool operator !=(RopePositionProperty a, RopePositionProperty b) => !a.Equals(b);
	}

	[PropertySerializer]
	public class RopePositionPropertySerializer : PropertySerializer<RopePositionProperty>
	{
		public override RopePositionProperty Serialize(GameObject instance)
		{
			return new RopePositionProperty
			{
				StartPosition = instance.transform.position,
				EndPosition = instance.transform.GetChild(0).position
			};
		}

		public override void Deserialize(RopePositionProperty property, GameObject target)
		{
			target.transform.position = property.StartPosition;
			target.transform.GetChild(0).position = property.EndPosition;

			var rope = target.GetComponent<MapObjet_Rope>();
			rope.OnJointAdded(joint =>
			{
				var distanceJoint = joint as DistanceJoint2D;
				if (distanceJoint)
				{
					rope.ExecuteAfterFrames(1, () => distanceJoint.autoConfigureDistance = false);
				}
			});
		}
	}
}
