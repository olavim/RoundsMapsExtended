using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SpawnData : MapObjectData
	{
		public SpawnIDProperty id = new SpawnIDProperty();
		public PositionProperty position = new PositionProperty();
	}

	[MapObject]
	public class Spawn : IMapObject<SpawnData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");

		public virtual void OnInstantiate(GameObject instance) { }
	}

	[PropertySerializer]
	public class SpawnIDPropertySerializer : PropertySerializer<SpawnIDProperty>
	{
		public override void Serialize(GameObject instance, SpawnIDProperty property)
		{
			var spawnPoint = instance.gameObject.GetComponent<SpawnPoint>();
			property.id = spawnPoint.ID;
			property.teamId = spawnPoint.TEAMID;
		}

		public override void Deserialize(SpawnIDProperty property, GameObject target)
		{
			var spawnPoint = target.gameObject.GetComponent<SpawnPoint>();
			spawnPoint.ID = property.id;
			spawnPoint.TEAMID = property.teamId;
		}
	}
}
