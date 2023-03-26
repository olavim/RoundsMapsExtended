using MapsExt.MapObjects.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SpawnIDProperty : IMapObjectProperty
	{
		public int Id { get; set; }
		public int TeamID { get; set; }
	}

	public class SpawnData : MapObjectData
	{
		public IMapObjectProperty Id { get; set; } = new SpawnIDProperty();
		public IMapObjectProperty Position { get; set; } = new PositionProperty();
	}

	[MapObject]
	public class Spawn : IMapObject<SpawnData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");
	}

	[MapObjectPropertySerializer]
	public class SpawnIDPropertySerializer : MapObjectPropertySerializer<SpawnIDProperty>
	{
		public override void Serialize(GameObject instance, SpawnIDProperty property)
		{
			var spawnPoint = instance.gameObject.GetComponent<SpawnPoint>();
			property.Id = spawnPoint.ID;
			property.TeamID = spawnPoint.TEAMID;
		}

		public override void Deserialize(SpawnIDProperty property, GameObject target)
		{
			var spawnPoint = target.gameObject.GetComponent<SpawnPoint>();
			spawnPoint.ID = property.Id;
			spawnPoint.TEAMID = property.TeamID;
		}
	}
}
