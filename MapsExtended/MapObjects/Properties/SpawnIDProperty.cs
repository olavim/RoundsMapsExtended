using System;

namespace MapsExt.MapObjects.Properties
{
	public class SpawnIDProperty : IProperty, IEquatable<SpawnIDProperty>
	{
		public int id;
		public int teamId;

		public SpawnIDProperty() : this(0, 0) { }

		public SpawnIDProperty(int id, int teamId)
		{
			this.id = id;
			this.teamId = teamId;
		}

		public bool Equals(SpawnIDProperty other) => this.id == other.id && this.teamId == other.teamId;
		public override bool Equals(object other) => other is SpawnIDProperty prop && this.Equals(prop);
		public override int GetHashCode() => (this.id, this.teamId).GetHashCode();

		public static bool operator ==(SpawnIDProperty a, SpawnIDProperty b) => a.Equals(b);
		public static bool operator !=(SpawnIDProperty a, SpawnIDProperty b) => !a.Equals(b);
	}
}
