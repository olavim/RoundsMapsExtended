using MapsExt.MapObjects;
using MapsExt.Properties;
using System.Collections.Generic;

namespace MapsExt.Tests
{
	internal static class SerializationTestUtils
	{
		public static MapObjectData PopulateTestData(MapObjectData data)
		{
			data.Active = true;
			data.TrySetProperty(new PositionProperty(1, 1));
			data.TrySetProperty(new ScaleProperty(4, 2));
			data.TrySetProperty(new RotationProperty(20));
			data.TrySetProperty(new DamageableProperty(false));
			data.TrySetProperty(new RopePositionProperty(new(5, 5), new(10, 10)));
			data.TrySetProperty(new SpawnIDProperty(5, 7));

			var frames = new List<AnimationKeyframe>();
			var linearProps = data.GetProperties<ILinearProperty>();

			for (int i = 0; i < 5; i++)
			{
				var frameValues = new List<ILinearProperty>();
				foreach (var linearProp in linearProps)
				{
					if (linearProp is PositionProperty pos)
					{
						frameValues.Add(pos + new PositionProperty(i, i));
					}

					if (linearProp is ScaleProperty scale)
					{
						frameValues.Add(scale + new ScaleProperty(i, i));
					}

					if (linearProp is RotationProperty rot)
					{
						frameValues.Add(rot + (i * 5));
					}
				}
				frames.Add(new AnimationKeyframe(frameValues.ToArray()));
			}

			data.TrySetProperty(new AnimationProperty(frames));
			return data;
		}
	}
}
