using MapsExt.MapObjects.Properties;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class RopeData : MapObjectData
	{
		public RopePositionProperty position = new RopePositionProperty();
	}

	[MapObject]
	public class Rope : IMapObject<RopeData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Rope");

		public virtual void OnInstantiate(GameObject instance) { }
	}

	[PropertySerializer]
	public class RopePositionPropertySerializer : PropertySerializer<RopePositionProperty>
	{
		public override void Serialize(GameObject instance, RopePositionProperty property)
		{
			property.startPosition = instance.transform.position;
			property.endPosition = instance.transform.GetChild(0).position;
		}

		public override void Deserialize(RopePositionProperty property, GameObject target)
		{
			target.transform.position = property.startPosition;
			target.transform.GetChild(0).position = property.endPosition;

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
