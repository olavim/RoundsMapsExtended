using MapsExt.MapObjects;
using MapsExt.Properties;
using System;
using UnityEngine;

#pragma warning disable CS0649

namespace MapsExt.Compatibility.V0.MapObjects
{
	[Obsolete("Deprecated")]
	internal abstract class MapObject : MapsExt.MapObjects.MapObject, IUpgradable
	{
		public abstract object Upgrade();

		public T Populate<T>(T data) where T : MapObjectData
		{
			data.Active = this.Active;
			return data;
		}
	}

	[Obsolete("Deprecated")]
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

	[Obsolete("Deprecated")]
	internal class Ball : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new BallData());
	}

	[Obsolete("Deprecated")]
	internal class Box : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new BoxData());
	}

	[Obsolete("Deprecated")]
	internal class BoxBackground : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new BoxBackgroundData());
	}

	[Obsolete("Deprecated")]
	internal class BoxDestructible : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new BoxDestructibleData());
	}

	[Obsolete("Deprecated")]
	internal class Ground : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new GroundData());
	}

	[Obsolete("Deprecated")]
	internal class GroundCircle : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new GroundCircleData());
	}

	[Obsolete("Deprecated")]
	internal class Saw : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new SawData());
	}

	[Obsolete("Deprecated")]
	internal class SawDynamic : SpatialMapObject
	{
		public override object Upgrade() => this.Populate(new SawDynamicData());
	}

	[Obsolete("Deprecated")]
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

	[Obsolete("Deprecated")]
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
