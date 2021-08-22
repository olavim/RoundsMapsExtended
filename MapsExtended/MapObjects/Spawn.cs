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

		protected override void OnDeserialize(Spawn data, GameObject target)
		{
			var spawnPoint = target.gameObject.GetComponent<SpawnPoint>();
			spawnPoint.ID = data.id;
			spawnPoint.TEAMID = data.teamID;
			target.transform.position = data.position;
		}

		protected override Spawn OnSerialize(GameObject instance)
		{
			var spawnPoint = instance.gameObject.GetComponent<SpawnPoint>();
			return new Spawn
			{
				id = spawnPoint.ID,
				teamID = spawnPoint.TEAMID,
				position = instance.transform.position
			};
		}
	}

	public class SpawnInstance : MapObjectInstance { };
}
