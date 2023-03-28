using FluentAssertions;
using MapsExt.Compatibility;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using Sirenix.Serialization;
using Surity;
using UnityEngine;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	public class OldVersionSerializationTests
	{
		private static readonly DeserializationContext deserializationContext = new DeserializationContext
		{
			Config = new SerializationConfig
			{
				DebugContext = new DebugContext
				{
					ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient,
					LoggingPolicy = LoggingPolicy.LogWarningsAndErrors,
					Logger = new SerializationLogger()
				}
			}
		};

		[Test]
		public void Test_LoadV0Map()
		{
			var map = MapLoader.LoadResource("MapsExt.Serialization.Fixtures.0.10.0.map", deserializationContext);
			map.Should().NotBeNull();
			map.mapObjects.Should().NotBeNull();
			map.mapObjects.Count.Should().Be(10);
			map.mapObjects.Should().AllBeAssignableTo(typeof(MapObjectData));

			var ballData = (BallData) map.mapObjects[0];
			ballData.active.Should().Be(true);
			ballData.position.Should().Be(new PositionProperty(-13.25f, 7.75f));
			ballData.scale.Should().Be(new ScaleProperty(4, 2));
			ballData.rotation.Should().Be(new RotationProperty());

			var boxData = (BoxData) map.mapObjects[1];
			boxData.active.Should().Be(true);
			boxData.position.Should().Be(new PositionProperty(-10.5f, 7.75f));
			boxData.scale.Should().Be(new ScaleProperty(2, 2));
			boxData.rotation.Should().Be(new RotationProperty(45));

			var boxBackgroundData = (BoxBackgroundData) map.mapObjects[2];
			boxBackgroundData.active.Should().Be(false);
			boxBackgroundData.position.Should().Be(new PositionProperty(-8f, 7.75f));
			boxBackgroundData.scale.Should().Be(new ScaleProperty(2, 2));
			boxBackgroundData.rotation.Should().Be(new RotationProperty());

			var boxDestructibleData = (BoxDestructibleData) map.mapObjects[3];
			boxDestructibleData.active.Should().Be(true);
			boxDestructibleData.DamageableByEnvironment.Should().Be(new DamageableProperty(true));
			boxDestructibleData.position.Should().Be(new PositionProperty(-5.5f, 7.75f));
			boxDestructibleData.scale.Should().Be(new ScaleProperty(2, 2));
			boxDestructibleData.rotation.Should().Be(new RotationProperty());

			var sawDynamicData = (SawDynamicData) map.mapObjects[4];
			sawDynamicData.active.Should().Be(true);
			sawDynamicData.position.Should().Be(new PositionProperty(-2.75f, 7.75f));
			sawDynamicData.scale.Should().Be(new ScaleProperty(2, 2));
			sawDynamicData.rotation.Should().Be(new RotationProperty());

			var groundData = (GroundData) map.mapObjects[5];
			groundData.active.Should().Be(true);
			groundData.position.Should().Be(new PositionProperty(-13.25f, 5.25f));
			groundData.scale.Should().Be(new ScaleProperty(2, 2));
			groundData.rotation.Should().Be(new RotationProperty());

			var sawData = (SawData) map.mapObjects[6];
			sawData.active.Should().Be(true);
			sawData.position.Should().Be(new PositionProperty(-8f, 5.25f));
			sawData.scale.Should().Be(new ScaleProperty(2, 2));
			sawData.rotation.Should().Be(new RotationProperty());

			var groundCircleData = (GroundCircleData) map.mapObjects[7];
			groundCircleData.active.Should().Be(true);
			groundCircleData.position.Should().Be(new PositionProperty(-10.5f, 5.25f));
			groundCircleData.scale.Should().Be(new ScaleProperty(2, 2));
			groundCircleData.rotation.Should().Be(new RotationProperty());

			var ropeData = (RopeData) map.mapObjects[8];
			ropeData.active.Should().Be(true);
			ropeData.position.Should().Be(new RopePositionProperty(new Vector2(-6.25f, 6f), new Vector2(-5f, 4.75f)));

			var spawnData = (SpawnData) map.mapObjects[9];
			spawnData.active.Should().Be(true);
			spawnData.id.Should().Be(new SpawnIDProperty(1, 2));
			spawnData.position.Should().Be(new PositionProperty(-2.75f, 5.5f));
		}
	}
}
