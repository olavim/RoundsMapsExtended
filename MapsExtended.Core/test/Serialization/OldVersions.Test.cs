using FluentAssertions;
using MapsExt.Compatibility;
using MapsExt.MapObjects;
using MapsExt.Properties;
using Sirenix.Serialization;
using Surity;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MapsExt.Tests
{
	[TestClass]
	public class OldVersionSerializationTests
	{
		private static readonly SerializationConfig serializationConfig = new()
		{
			DebugContext = new DebugContext
			{
				ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient,
				LoggingPolicy = LoggingPolicy.LogWarningsAndErrors,
				Logger = new SerializationLogger()
			}
		};

		[Test]
		public void Test_LoadV0Map()
		{
			var map = MapLoader.LoadResource("MapsExt.Serialization.Fixtures.0.10.0.map", new() { Config = serializationConfig });
			map.Should().NotBeNull();
			map.MapObjects.Should().NotBeNull();
			map.MapObjects.Length.Should().Be(10);
			map.MapObjects.Should().AllBeAssignableTo(typeof(MapObjectData));

			var ballData = (BallData) map.MapObjects[0];
			ballData.Active.Should().Be(true);
			ballData.Position.Should().Be(new PositionProperty(-13.25f, 7.75f));
			ballData.Scale.Should().Be(new ScaleProperty(4, 2));
			ballData.Rotation.Should().Be(new RotationProperty());

			var boxData = (BoxData) map.MapObjects[1];
			boxData.Active.Should().Be(true);
			boxData.Position.Should().Be(new PositionProperty(-10.5f, 7.75f));
			boxData.Scale.Should().Be(new ScaleProperty(2, 2));
			boxData.Rotation.Should().Be(new RotationProperty(45));

			var boxBackgroundData = (BoxBackgroundData) map.MapObjects[2];
			boxBackgroundData.Active.Should().Be(false);
			boxBackgroundData.Position.Should().Be(new PositionProperty(-8f, 7.75f));
			boxBackgroundData.Scale.Should().Be(new ScaleProperty(2, 2));
			boxBackgroundData.Rotation.Should().Be(new RotationProperty());

			var boxDestructibleData = (BoxDestructibleData) map.MapObjects[3];
			boxDestructibleData.Active.Should().Be(true);
			boxDestructibleData.DamageableByEnvironment.Should().Be(new DamageableProperty(true));
			boxDestructibleData.Position.Should().Be(new PositionProperty(-5.5f, 7.75f));
			boxDestructibleData.Scale.Should().Be(new ScaleProperty(2, 2));
			boxDestructibleData.Rotation.Should().Be(new RotationProperty());

			var sawDynamicData = (SawDynamicData) map.MapObjects[4];
			sawDynamicData.Active.Should().Be(true);
			sawDynamicData.Position.Should().Be(new PositionProperty(-2.75f, 7.75f));
			sawDynamicData.Scale.Should().Be(new ScaleProperty(2, 2));
			sawDynamicData.Rotation.Should().Be(new RotationProperty());

			var groundData = (GroundData) map.MapObjects[5];
			groundData.Active.Should().Be(true);
			groundData.Position.Should().Be(new PositionProperty(-13.25f, 5.25f));
			groundData.Scale.Should().Be(new ScaleProperty(2, 2));
			groundData.Rotation.Should().Be(new RotationProperty());

			var sawData = (SawData) map.MapObjects[6];
			sawData.Active.Should().Be(true);
			sawData.Position.Should().Be(new PositionProperty(-8f, 5.25f));
			sawData.Scale.Should().Be(new ScaleProperty(2, 2));
			sawData.Rotation.Should().Be(new RotationProperty());

			var groundCircleData = (GroundCircleData) map.MapObjects[7];
			groundCircleData.Active.Should().Be(true);
			groundCircleData.Position.Should().Be(new PositionProperty(-10.5f, 5.25f));
			groundCircleData.Scale.Should().Be(new ScaleProperty(2, 2));
			groundCircleData.Rotation.Should().Be(new RotationProperty());

			var ropeData = (RopeData) map.MapObjects[8];
			ropeData.Active.Should().Be(true);
			ropeData.Position.Should().Be(new RopePositionProperty(new Vector2(-6.25f, 6f), new Vector2(-5f, 4.75f)));

			var spawnData = (SpawnData) map.MapObjects[9];
			spawnData.Active.Should().Be(true);
			spawnData.Id.Should().Be(new SpawnIDProperty(1, 2));
			spawnData.Position.Should().Be(new PositionProperty(-2.75f, 5.5f));
		}

		[Test]
		public void Test_LoadV0MapWithCustomMapObjects()
		{
#pragma warning disable CS0618
			var map1 = new Compatibility.V0.CustomMap
			{
				id = "test",
				name = "test",
				mapObjects = new List<MapObject>
				{
					new V0Box() {
						Active = true,
						position = new Vector2(1, 1),
						scale = new Vector2(1, 1),
						rotation = Quaternion.Euler(0, 0, 45)
					}
				}
			};
#pragma warning restore CS0618

			var bytes = SerializationUtility.SerializeValue(map1, DataFormat.JSON, new() { Config = serializationConfig });
			using var stream = new MemoryStream(bytes);
			var map2 = MapLoader.Load(stream);

			map2.Should().NotBeNull();
			map2.Name.Should().Be("test");
			map2.Id.Should().Be("test");
			map2.MapObjects.Should().NotBeNull();
			map2.MapObjects.Length.Should().Be(1);
			map2.MapObjects.Should().AllBeAssignableTo(typeof(MapObjectData));

			var box1 = (V0Box) map1.mapObjects[0];
			var box2 = (V0Box) map2.MapObjects[0];

			box2.Active.Should().Be(box1.Active);
			box2.position.Should().Be(box1.position);
			box2.scale.Should().Be(box1.scale);
			box2.rotation.Should().Be(box1.rotation);
		}
	}
}
