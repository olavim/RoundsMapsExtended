using MapsExt.MapObjects.Properties;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class RopePositionProperty : IMapObjectProperty
	{
		public Vector2 StartPosition { get; set; } = Vector2.up;
		public Vector2 EndPosition { get; set; } = Vector2.down;
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
