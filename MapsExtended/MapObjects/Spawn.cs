using UnityEngine;

namespace MapsExt.MapObjects
{
	public class Spawn : MapObject
	{
		public int id = 0;
		public int teamID = 0;
		public Vector3 position = Vector3.zero;
	}

	[MapsExtendedMapObject(typeof(Spawn))]
	public class SpawnSpecification : MapObjectSpecification<Spawn>
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");

		protected override void Deserialize(Spawn data, GameObject target)
		{
			var spawnPoint = target.gameObject.GetComponent<SpawnPoint>();
			spawnPoint.ID = data.id;
			spawnPoint.TEAMID = data.teamID;
			target.transform.position = data.position;
		}

		protected override void Serialize(GameObject instance, Spawn target)
		{
			var spawnPoint = instance.gameObject.GetComponent<SpawnPoint>();
			target.id = spawnPoint.ID;
			target.teamID = spawnPoint.TEAMID;
			target.position = instance.transform.position;
		}
	}

	public class SpawnInstance : MapObjectInstance { };
}
