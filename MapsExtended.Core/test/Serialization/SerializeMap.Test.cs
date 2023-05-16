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
			var a = new CustomMap("test", "Test", "1.0.0", new[] {
				SerializationTestUtils.PopulateTestData(new BallData())
			});

			var bytes = SerializationUtility.SerializeValue(a, DataFormat.JSON, serializationContext);
			using var stream = new MemoryStream(bytes);
			var b = SerializationUtility.DeserializeValue<CustomMap>(stream, DataFormat.JSON, deserializationContext);

			a.Should().NotBeNull();
			b.Should().NotBeNull();

			a.Id.Should().Be(b.Id);
			a.Name.Should().Be(b.Name);

			a.MapObjects.Should().NotBeNull();
			b.MapObjects.Should().NotBeNull();

			a.MapObjects.Length.Should().Be(b.MapObjects.Length);
			a.MapObjects[0].Should().BeOfType(typeof(BallData));
			b.MapObjects[0].Should().BeOfType(typeof(BallData));

			var props1 = a.MapObjects[0].GetProperties<IProperty>();

			for (int j = 0; j < props1.Length; j++)
			{
				var prop1 = props1[j];
				var prop2 = b.MapObjects[0].GetProperty(prop1.GetType());

				if (prop1 is AnimationProperty animProp1 && prop2 is AnimationProperty animProp2)
				{
					AssertEquals(animProp1, animProp2);
				}
				else
				{
					prop1.Should().Be(prop2);
				}
			}
		}

		private static void AssertEquals(AnimationProperty a, AnimationProperty b)
		{
			a.Keyframes.Length.Should().Be(b.Keyframes.Length);
			for (int i = 0; i < a.Keyframes.Length; i++)
			{
				a.Keyframes[i].ComponentValues.Count.Should().Be(b.Keyframes[i].ComponentValues.Count);
				for (int j = 0; j < a.Keyframes[i].ComponentValues.Count; j++)
				{
					var prop1 = a.Keyframes[i].ComponentValues[j];
					var prop2 = b.Keyframes[i].GetComponentValue(prop1.GetType());
					prop1.Should().Be(prop2);
				}
			}
		}
	}
}
