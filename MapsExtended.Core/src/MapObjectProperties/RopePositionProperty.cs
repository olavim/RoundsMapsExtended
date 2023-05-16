using UnityEngine;
using System;
using UnboundLib;

namespace MapsExt.Properties
{
	public class RopePositionProperty : IProperty, IEquatable<RopePositionProperty>
	{
		[SerializeField] private Vector2 _pos1;
		[SerializeField] private Vector2 _pos2;

		public Vector2 StartPosition { get => this._pos1; set => this._pos1 = value; }
		public Vector2 EndPosition { get => this._pos2; set => this._pos2 = value; }

		public RopePositionProperty()
		{
			var pos = (Vector2) MainCam.instance.cam.transform.position;
			this._pos1 = pos + Vector2.up;
			this._pos2 = pos + Vector2.down;
		}

		public RopePositionProperty(Vector2 startPosition, Vector2 endPosition)
		{
			this._pos1 = startPosition;
			this._pos2 = endPosition;
		}

		public bool Equals(RopePositionProperty other) =>
			this.StartPosition.Equals(other.StartPosition) && this.EndPosition.Equals(other.EndPosition);
		public override bool Equals(object other) => other is RopePositionProperty prop && this.Equals(prop);
		public override int GetHashCode() => (this.StartPosition, this.EndPosition).GetHashCode();

		public static bool operator ==(RopePositionProperty a, RopePositionProperty b) => a.Equals(b);
		public static bool operator !=(RopePositionProperty a, RopePositionProperty b) => !a.Equals(b);
	}

	[PropertySerializer(typeof(RopePositionProperty))]
	public class RopePositionPropertySerializer : IPropertyWriter<RopePositionProperty>
	{
		public virtual void WriteProperty(RopePositionProperty property, GameObject target)
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
