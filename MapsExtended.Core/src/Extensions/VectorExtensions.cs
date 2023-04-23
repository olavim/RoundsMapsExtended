using UnityEngine;

namespace MapsExt
{
	public static class VectorExtensions
	{
		public static Vector3 Round(this Vector3 vector, int decimalPlaces)
		{
			return new(
				(float) System.Math.Round(vector.x, decimalPlaces),
				(float) System.Math.Round(vector.y, decimalPlaces),
				(float) System.Math.Round(vector.z, decimalPlaces)
			);
		}

		public static Vector2 Round(this Vector2 vector, int decimalPlaces)
		{
			return new(
				(float) System.Math.Round(vector.x, decimalPlaces),
				(float) System.Math.Round(vector.y, decimalPlaces)
			);
		}
	}
}
