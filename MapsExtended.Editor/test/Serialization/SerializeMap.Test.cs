using FluentAssertions;
using MapsExt.MapObjects;
using MapsExt.MapObjects.Properties;
using Sirenix.Serialization;
using Surity;
using System.Collections.Generic;
using System.IO;

namespace MapsExt.Editor.Tests
{
	[TestClass]
	public class MapSerializationTests
	{
		private static readonly SerializationConfig serializationConfig = new SerializationConfig
		{
			SerializationPolicy = SerializationPolicies.Everything,
			AllowDeserializeInvalidData = true,
			DebugContext = new DebugContext
			{
				ErrorHandlingPolicy = ErrorHandlingPolicy.Resilient,
				LoggingPolicy = LoggingPolicy.LogWarningsAndErrors,
				Logger = new SerializationLogger()
			}
		};

		private static readonly SerializationContext serializationContext = new SerializationContext { Config = serializationConfig };
		private static readonly DeserializationContext deserializationContext = new DeserializationContext { Config = serializationConfig };

		[Test]
		public void Test_SerializeMap()
		{
			var ballData = new BallData
			{
				active = true,
				position = new PositionProperty(1, 1),
				scale = new ScaleProperty(4, 2),
				rotation = new RotationProperty(20)
			};

			var map = new CustomMap
			{
				id = "test",
				name = "Test",
				version = "1.0.0",
				mapObjects = new List<MapObjectData> { ballData }
			};

			var bytes = SerializationUtility.SerializeValue(map, DataFormat.JSON, serializationContext);
			using (var stream = new MemoryStream(bytes))
			{
				var deserializedMap = SerializationUtility.DeserializeValue<CustomMap>(stream, DataFormat.JSON, deserializationContext);

				deserializedMap.Should().NotBeNull();
				deserializedMap.id.Should().Be(map.id);
				deserializedMap.name.Should().Be(map.name);
				deserializedMap.mapObjects.Should().NotBeNull();
				deserializedMap.mapObjects.Count.Should().Be(1);
				deserializedMap.mapObjects.Should().AllBeAssignableTo(typeof(MapObjectData));

				var deserializedBallData = (BallData) deserializedMap.mapObjects[0];
				deserializedBallData.Should().BeOfType<BallData>();
				deserializedBallData.active.Should().Be(ballData.active);
				deserializedBallData.position.Should().Be(ballData.position);
				deserializedBallData.scale.Should().Be(ballData.scale);
				deserializedBallData.rotation.Should().Be(ballData.rotation);
			}
		}
	}
}
