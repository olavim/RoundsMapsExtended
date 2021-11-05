using UnityEngine;
using HarmonyLib;
using UnboundLib;

namespace MapsExt.MapObjects
{
	public class Rope : MapObject
	{
		public Vector3 startPosition = Vector3.up;
		public Vector3 endPosition = Vector3.down;

		public override MapObject Move(Vector3 v)
		{
			var copy = (Rope) AccessTools.Constructor(this.GetType()).Invoke(new object[] { });
			copy.active = this.active;
			copy.startPosition = this.startPosition + v;
			copy.endPosition = this.endPosition + v;
			return copy;
		}

		public override string ToString()
		{
			return $"Rope[{this.startPosition}, {this.endPosition}]";
		}
	}

	[MapObjectSpec(typeof(Rope))]
	public static class RopeSpec
	{
		[MapObjectPrefab]
		public static GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Rope");

		[MapObjectSerializer]
		public static void Serialize(GameObject instance, Rope target)
		{
			target.startPosition = instance.transform.position;
			target.endPosition = instance.transform.GetChild(0).position;
		}

		[MapObjectDeserializer]
		public static void Deserialize(Rope data, GameObject target)
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
