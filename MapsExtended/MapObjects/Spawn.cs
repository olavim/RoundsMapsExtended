using UnityEngine;

namespace MapsExt.MapObjects
{
	public class Spawn : MapObject
	{
		public int id = 0;
		public int teamID = 0;
		public Vector3 position = Vector3.zero;
	}

	[MapObjectBlueprint]
	public class SpawnBP : BaseMapObjectBlueprint<Spawn>
	{
		public override GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");

		public override void Serialize(GameObject instance, Spawn target)
		{
			var spawnPoint = instance.gameObject.GetComponent<SpawnPoint>();
			target.id = spawnPoint.ID;
			target.teamID = spawnPoint.TEAMID;
			target.position = instance.transform.position;
		}

		public override void Deserialize(Spawn data, GameObject target)
		{
			var spawnPoint = target.gameObject.GetComponent<SpawnPoint>();
			spawnPoint.ID = data.id;
			spawnPoint.TEAMID = data.teamID;
			spawnPoint.localStartPos = data.position;
			target.transform.position = data.position;
		}
	}

	public class SpawnInstance : MapObjectInstance { };
}
