using UnityEngine;

namespace MapsExt.Compatibility.V0.MapObjects
{
	public abstract class MapObject : IUpgradable
	{
		public bool active = true;

		public abstract object Upgrade();

		public T Populate<T>(T data) where T : MapsExt.MapObjects.MapObjectData
		{
			data.active = this.active;
			return data;
		}
	}

	public abstract class SpatialMapObject : MapObject
	{
		public Vector3 position;
		public Vector3 scale;
		public Quaternion rotation;

		public new T Populate<T>(T data) where T : MapsExt.MapObjects.SpatialMapObjectData
		{
			base.Populate(data);
			data.Position = this.position;
			data.Scale = this.scale;
			data.Rotation = this.rotation;
			return data;
		}
	}

	public class Ball : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new MapsExt.MapObjects.BallData());
	}

	public class Box : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new MapsExt.MapObjects.BoxData());
	}

	public class BoxBackground : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new MapsExt.MapObjects.BoxBackgroundData());
	}

	public class BoxDestructible : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new MapsExt.MapObjects.BoxDestructibleData());
	}

	public class Ground : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new MapsExt.MapObjects.GroundData());
	}

	public class GroundCircle : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new MapsExt.MapObjects.GroundCircleData());
	}

	public class Saw : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new MapsExt.MapObjects.SawData());
	}

	public class SawDynamic : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new MapsExt.MapObjects.SawDynamicData());
	}

	public class Rope : MapObject
	{
		public Vector3 startPosition = Vector3.up;
		public Vector3 endPosition = Vector3.down;

		public override object Upgrade()
		{
			var data = this.Populate(new MapsExt.MapObjects.RopeData());
			data.Position = new MapsExt.MapObjects.RopePositionProperty
			{
				StartPosition = this.startPosition,
				EndPosition = this.endPosition
			};
			return data;
		}
	}

	public class Spawn : MapObject
	{
		public int id;
		public int teamID;
		public Vector3 position;

		public override object Upgrade()
		{
			var data = this.Populate(new MapsExt.MapObjects.SpawnData());
			data.Id = new MapsExt.MapObjects.SpawnIDProperty
			{
				Id = this.id,
				TeamID = this.teamID
			};
			data.Position = this.position;
			return data;
		}
	}
}
