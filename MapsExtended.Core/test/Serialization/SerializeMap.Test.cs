using FluentAssertions;
using MapsExt.MapObjects;
using MapsExt.Properties;
using Sirenix.Serialization;
using Surity;
using System.IO;

namespace MapsExt.Tests
{
	[TestClass]
	public class MapSerializationTests
	{
		private static readonly SerializationConfig serializationConfig = new()
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

		private static readonly SerializationContext serializationContext = new() { Config = serializationConfig };
		private static readonly DeserializationContext deserializationContext = new() { Config = serializationConfig };

		[Test]
		public void Test_SerializeMap()
		{
			var ballData = new BallData
			{
				active = true,
				Position = new PositionProperty(1, 1),
				Scale = new ScaleProperty(4, 2),
				Rotation = new RotationProperty(20),
				Animation = new AnimationProperty(
					new AnimationKeyframe[] {
						new AnimationKeyframe(new PositionProperty(1, 1), new ScaleProperty(4, 2), new RotationProperty(20)),
						new AnimationKeyframe(new PositionProperty(2, 2), new ScaleProperty(2, 2), new RotationProperty(40))
					}
				)
			};

			var map = new CustomMap("test", "Test", "1.0.0", new[] { ballData });

			var bytes = SerializationUtility.SerializeValue(map, DataFormat.JSON, serializationContext);
			using var stream = new MemoryStream(bytes);
			var deserializedMap = SerializationUtility.DeserializeValue<CustomMap>(stream, DataFormat.JSON, deserializationContext);

			deserializedMap.Should().NotBeNull();
			deserializedMap.Id.Should().Be(map.Id);
			deserializedMap.Name.Should().Be(map.Name);
			deserializedMap.MapObjects.Should().NotBeNull();
			deserializedMap.MapObjects.Length.Should().Be(1);
			deserializedMap.MapObjects.Should().AllBeAssignableTo(typeof(MapObjectData));

			var deserializedBallData = (BallData) deserializedMap.MapObjects[0];
			deserializedBallData.Should().BeOfType<BallData>();
			deserializedBallData.active.Should().Be(ballData.active);
			deserializedBallData.Position.Should().Be(ballData.Position);
			deserializedBallData.Scale.Should().Be(ballData.Scale);
			deserializedBallData.Rotation.Should().Be(ballData.Rotation);
			deserializedBallData.Animation.Keyframes[0].ComponentValues[0].Should().Be(ballData.Animation.Keyframes[0].ComponentValues[0]);
			deserializedBallData.Animation.Keyframes[0].ComponentValues[1].Should().Be(ballData.Animation.Keyframes[0].ComponentValues[1]);
			deserializedBallData.Animation.Keyframes[0].ComponentValues[2].Should().Be(ballData.Animation.Keyframes[0].ComponentValues[2]);
			deserializedBallData.Animation.Keyframes[1].ComponentValues[0].Should().Be(ballData.Animation.Keyframes[1].ComponentValues[0]);
			deserializedBallData.Animation.Keyframes[1].ComponentValues[1].Should().Be(ballData.Animation.Keyframes[1].ComponentValues[1]);
			deserializedBallData.Animation.Keyframes[1].ComponentValues[2].Should().Be(ballData.Animation.Keyframes[1].ComponentValues[2]);
		}
	}
}
