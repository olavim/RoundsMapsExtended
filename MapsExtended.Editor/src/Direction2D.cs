using UnityEngine;

namespace MapsExt.Editor
{
	public class Direction2D
	{
		public static readonly Direction2D Middle = new(new(0, 0));
		public static readonly Direction2D North = new(new(0, 1f));
		public static readonly Direction2D South = new(new(0, -1f));
		public static readonly Direction2D East = new(new(1f, 0));
		public static readonly Direction2D West = new(new(-1f, 0));
		public static readonly Direction2D NorthEast = new(new(1f, 1f));
		public static readonly Direction2D NorthWest = new(new(-1f, 1f));
		public static readonly Direction2D SouthEast = new(new(1f, -1f));
		public static readonly Direction2D SouthWest = new(new(-1f, -1f));

		private Vector2 directionMultiplier;

		private Direction2D(Vector2 directionMultiplier)
		{
			this.directionMultiplier = directionMultiplier;
		}

		public override string ToString()
		{
			return this switch
			{
				var _ when this == Middle => "Middle",
				var _ when this == North => "North",
				var _ when this == South => "South",
				var _ when this == East => "East",
				var _ when this == West => "West",
				var _ when this == NorthEast => "NorthEast",
				var _ when this == NorthWest => "NorthWest",
				var _ when this == SouthEast => "SouthEast",
				var _ when this == SouthWest => "SouthWest",
				_ => base.ToString(),
			};
		}

		public static Vector2 operator *(Direction2D dir, Vector2 v) => Vector2.Scale(v, dir.directionMultiplier);
		public static Vector2 operator *(Vector2 v, Direction2D dir) => Vector2.Scale(v, dir.directionMultiplier);
	}
}
