using System;
using UnityEngine;

namespace MapsExt.Properties
{
	public class SpawnIDProperty : IProperty, IEquatable<SpawnIDProperty>
	{
		[SerializeField] private int _id;
		[SerializeField] private int _teamId;

		public int Id { get => this._id; set => this._id = value; }
		public int TeamId { get => this._teamId; set => this._teamId = value; }

		public SpawnIDProperty() : this(0, 0) { }

		public SpawnIDProperty(int id, int teamId)
		{
			this.Id = id;
			this.TeamId = teamId;
		}

		public bool Equals(SpawnIDProperty other) => this.Id == other.Id && this.TeamId == other.TeamId;
		public override bool Equals(object other) => other is SpawnIDProperty prop && this.Equals(prop);
		public override int GetHashCode() => (this.Id, this.TeamId).GetHashCode();

		public static bool operator ==(SpawnIDProperty a, SpawnIDProperty b) => a.Equals(b);
		public static bool operator !=(SpawnIDProperty a, SpawnIDProperty b) => !a.Equals(b);
	}

	[PropertySerializer(typeof(SpawnIDProperty))]
	public class SpawnIDPropertySerializer : IPropertyWriter<SpawnIDProperty>
	{
		public virtual void WriteProperty(SpawnIDProperty property, GameObject target)
		{
			var spawnPoint = target.gameObject.GetComponent<SpawnPoint>();
			spawnPoint.ID = property.Id;
			spawnPoint.TEAMID = property.TeamId;
		}
	}
}
