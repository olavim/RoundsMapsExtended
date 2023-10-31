using FluentAssertions;
using MapsExt.MapObjects;
using MapsExt.Properties;
using Sirenix.Serialization;
using Surity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MapsExt.Tests
{
	[TestClass]
	public class MapObjectSerializationTests
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

		[TestGenerator]
		public IEnumerable<TestInfo> Gen_SerializeMapObjects()
		{
			var types = typeof(MapsExtended).Assembly.GetTypes().Where(t => t.GetCustomAttribute<MapObjectAttribute>() != null);
			foreach (var type in types)
			{
				var dataType = type.GetCustomAttribute<MapObjectAttribute>().DataType;
				yield return new TestInfo($"Gen_SerializeMapObjects > {dataType.Name}", () => this.Test_SerializeMapObject(dataType));
			}
		}

		public void Test_SerializeMapObject(Type dataType)
		{
			var data1 = (MapObjectData) Activator.CreateInstance(dataType);
			SerializationTestUtils.PopulateTestData(data1);

			var bytes = SerializationUtility.SerializeValue(data1, DataFormat.JSON, serializationContext);
			using var stream = new MemoryStream(bytes);
			var data2 = SerializationUtility.DeserializeValue<MapObjectData>(stream, DataFormat.JSON, deserializationContext);

			data1.Should().BeOfType(data2.GetType());

			var props1 = data1.GetProperties<IProperty>();

			for (int j = 0; j < props1.Length; j++)
			{
				var prop1 = props1[j];
				var prop2 = data2.GetProperty(prop1.GetType());

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
