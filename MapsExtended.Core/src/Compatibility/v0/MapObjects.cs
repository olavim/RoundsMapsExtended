using MapsExt.MapObjects;
using MapsExt.Properties;
using System;
using UnityEngine;

#pragma warning disable CS0649

namespace MapsExt.Compatibility.V0.MapObjects
{
	[Obsolete("Deprecated")]
	internal abstract class MapObject
	{
		public bool active;

		public T Populate<T>(T data) where T : MapObjectData
		{
			data.Active = this.active;
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
			data.Scale = (ScaleProperty) this.scale;
			data.Rotation = (int) (this.rotation.eulerAngles.z * 100) / 100f;
			return data;
		}
	}

	[Obsolete("Deprecated")]
	internal class Ball : SpatialMapObject
	{
		public static implicit operator BallData(Ball data) => data.Populate(new BallData());
	}

	[Obsolete("Deprecated")]
	internal class Box : SpatialMapObject
	{
		public static implicit operator BoxData(Box data) => data.Populate(new BoxData());
	}

	[Obsolete("Deprecated")]
	internal class BoxBackground : SpatialMapObject
	{
		public static implicit operator BoxBackgroundData(BoxBackground data) => data.Populate(new BoxBackgroundData());
	}

	[Obsolete("Deprecated")]
	internal class BoxDestructible : SpatialMapObject
	{
		public static implicit operator BoxDestructibleData(BoxDestructible data) => data.Populate(new BoxDestructibleData());
	}

	[Obsolete("Deprecated")]
	internal class Ground : SpatialMapObject
	{
		public static implicit operator GroundData(Ground data) => data.Populate(new GroundData());
	}

	[Obsolete("Deprecated")]
	internal class GroundCircle : SpatialMapObject
	{
		public static implicit operator GroundCircleData(GroundCircle data) => data.Populate(new GroundCircleData());
	}

	[Obsolete("Deprecated")]
	internal class Saw : SpatialMapObject
	{
		public static implicit operator SawData(Saw data) => data.Populate(new SawData());
	}

	[Obsolete("Deprecated")]
	internal class SawDynamic : SpatialMapObject
	{
		public static implicit operator SawDynamicData(SawDynamic data) => data.Populate(new SawDynamicData());
	}

	[Obsolete("Deprecated")]
	internal class Rope : MapObject
	{
		public Vector3 startPosition = Vector3.up;
		public Vector3 endPosition = Vector3.down;

		public static implicit operator RopeData(Rope rope)
		{
			var data = rope.Populate(new RopeData());
			data.Position = new RopePositionProperty
			{
				StartPosition = rope.startPosition,
				EndPosition = rope.endPosition
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

		public static implicit operator SpawnData(Spawn spawn)
		{
			var data = spawn.Populate(new SpawnData());
			data.Id = new SpawnIDProperty
			{
				Id = spawn.id,
				TeamId = spawn.teamID
			};
			data.Position = spawn.position;
			return data;
		}
	}
}

#pragma warning restore CS0649
