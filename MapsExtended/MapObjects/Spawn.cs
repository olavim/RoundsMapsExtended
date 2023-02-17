using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SpawnData : MapObjectData, IMapObjectPosition
	{
		public int id = 0;
		public int teamID = 0;
		public Vector3 position { get; set; } = Vector3.zero;
	}

	[MapObject]
	public class Spawn : IMapObject<SpawnData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");
	}

	[MapObjectProperty]
	public class SpawnProperty : IMapObjectProperty<SpawnData>
	{
		public virtual void Serialize(GameObject instance, SpawnData target)
		{
			var spawnPoint = instance.gameObject.GetComponent<SpawnPoint>();
			target.id = spawnPoint.ID;
			target.teamID = spawnPoint.TEAMID;
		}

		public virtual void Deserialize(SpawnData data, GameObject target)
		{
			var spawnPoint = target.gameObject.GetComponent<SpawnPoint>();
			spawnPoint.ID = data.id;
			spawnPoint.TEAMID = data.teamID;
			spawnPoint.localStartPos = data.position;
		}
	}
}
