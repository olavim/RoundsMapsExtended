using UnityEngine;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class Rope : MapObject
	{
		public Vector3 startPosition = Vector3.up;
		public Vector3 endPosition = Vector3.down;

		public override string ToString()
		{
			return $"Rope[{this.startPosition}, {this.endPosition}]";
		}
	}

	[MapObjectBlueprint]
	public class RopeBP : BaseMapObjectBlueprint<Rope>
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Rope");

		public override void Serialize(GameObject instance, Rope target)
		{
			target.startPosition = instance.transform.position;
			target.endPosition = instance.transform.GetChild(0).position;
		}

		public override void Deserialize(Rope data, GameObject target)
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
