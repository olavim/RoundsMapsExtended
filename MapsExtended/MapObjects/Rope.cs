using UnityEngine;

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

	[MapsExtendedMapObject(typeof(Rope))]
	public class RopeSpecification : MapObjectSpecification<Rope>
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Rope");

		protected override void Deserialize(Rope data, GameObject target)
		{
			target.transform.position = data.startPosition;
			target.transform.GetChild(0).position = data.endPosition;
		}

		protected override void Serialize(GameObject instance, Rope target)
		{
			target.startPosition = instance.transform.position;
			target.endPosition = instance.transform.GetChild(0).position;
		}
	}
}
