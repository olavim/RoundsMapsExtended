using MapsExt.MapObjects.Properties;
using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class RopeData : MapObjectData
	{
		public Vector3 startPosition = Vector3.up;
		public Vector3 endPosition = Vector3.down;

		public override string ToString()
		{
			return $"Rope[{this.startPosition}, {this.endPosition}]";
		}
	}

	[MapObject]
	public class Rope : IMapObject<RopeData>
	{
		public GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Rope");
	}

	[MapObjectProperty]
	public class RopeSerializer : IMapObjectProperty<RopeData>
	{
		public void Serialize(GameObject instance, RopeData target)
		{
			target.startPosition = instance.transform.position;
			target.endPosition = instance.transform.GetChild(0).position;
		}

		public void Deserialize(RopeData data, GameObject target)
		{
			target.transform.position = data.startPosition;
			target.transform.GetChild(0).position = data.endPosition;

			var rope = target.GetComponent<MapObjet_Rope>();
			rope.OnJointAdded(joint =>
			{
				var distanceJoint = joint as DistanceJoint2D;
				if (distanceJoint)
				{
					rope.ExecuteAfterFrames(1, () =>
					{
						distanceJoint.autoConfigureDistance = false;
					});
				}
			});
		}
	}
}
