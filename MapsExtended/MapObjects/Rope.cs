using MapsExt.MapObjects.Properties;
using UnityEngine;
using UnboundLib;
using System;

namespace MapsExt.MapObjects
{
	public class RopePositionProperty : IMapObjectProperty, IEquatable<RopePositionProperty>
	{
		public Vector2 StartPosition { get; set; }
		public Vector2 EndPosition { get; set; }

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

	public class RopeData : MapObjectData
	{
		public RopePositionProperty Position = new RopePositionProperty();
	}

	[MapObject]
	public class Rope : IMapObject<RopeData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Rope");
	}

	[MapObjectPropertySerializer]
	public class RopePositionPropertySerializer : MapObjectPropertySerializer<RopePositionProperty>
	{
		public override void Serialize(GameObject instance, RopePositionProperty property)
		{
			property.StartPosition = instance.transform.position;
			property.EndPosition = instance.transform.GetChild(0).position;
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
