using MapsExt.MapObjects.Properties;
using System;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SpawnIDProperty : IProperty, IEquatable<SpawnIDProperty>
	{
		public int Id { get; set; }
		public int TeamID { get; set; }

		public SpawnIDProperty() : this(0, 0) { }

		public SpawnIDProperty(int id, int teamId)
		{
			this.Id = id;
			this.TeamID = teamId;
		}

		public bool Equals(SpawnIDProperty other) => this.Id == other.Id && this.TeamID == other.TeamID;
		public override bool Equals(object other) => other is SpawnIDProperty prop && this.Equals(prop);
		public override int GetHashCode() => (this.Id, this.TeamID).GetHashCode();

		public static bool operator ==(SpawnIDProperty a, SpawnIDProperty b) => a.Equals(b);
		public static bool operator !=(SpawnIDProperty a, SpawnIDProperty b) => !a.Equals(b);
	}

	public class SpawnData : MapObjectData
	{
		public SpawnIDProperty Id { get; set; } = new SpawnIDProperty();
		public PositionProperty Position { get; set; } = new PositionProperty();
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
