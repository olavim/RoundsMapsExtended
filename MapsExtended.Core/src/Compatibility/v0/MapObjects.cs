using MapsExt.MapObjects;
using MapsExt.Properties;
using UnityEngine;

#pragma warning disable CS0649

namespace MapsExt.Compatibility.V0.MapObjects
{
#pragma warning disable CS0618
	internal abstract class MapObject : MapsExt.MapObjects.MapObject, IUpgradable
#pragma warning restore CS0618
	{
		public abstract object Upgrade();

		public T Populate<T>(T data) where T : MapObjectData
		{
			data.active = this.active;
			return data;
		}
	}

	internal abstract class SpatialMapObject : MapObject
	{
		public Vector3 position;
		public Vector3 scale;
		public Quaternion rotation;

		public new T Populate<T>(T data) where T : SpatialMapObjectData
		{
			base.Populate(data);
			data.Position = this.position;
			data.Scale = this.scale;
			data.Rotation = (int) (this.rotation.eulerAngles.z * 100) / 100f;
			return data;
		}
	}

	internal class Ball : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new BallData());
	}

	internal class Box : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new BoxData());
	}

	internal class BoxBackground : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new BoxBackgroundData());
	}

	internal class BoxDestructible : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new BoxDestructibleData());
	}

	internal class Ground : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new GroundData());
	}

	internal class GroundCircle : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new GroundCircleData());
	}

	internal class Saw : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new SawData());
	}

	internal class SawDynamic : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new SawDynamicData());
	}

	internal class Rope : MapObject
	{
		public Vector3 startPosition = Vector3.up;
		public Vector3 endPosition = Vector3.down;

		public override object Upgrade()
		{
			var data = this.Populate(new RopeData());
			data.Position = new RopePositionProperty
			{
				StartPosition = this.startPosition,
				EndPosition = this.endPosition
			};
			return data;
		}
	}

	internal class Spawn : MapObject
	{
		public int id;
		public int teamID;
		public Vector3 position;

		public override object Upgrade()
		{
			var data = this.Populate(new SpawnData());
			data.Id = new SpawnIDProperty
			{
				Id = this.id,
				TeamId = this.teamID
			};
			data.Position = this.position;
			return data;
		}
	}
}

#pragma warning restore CS0649
