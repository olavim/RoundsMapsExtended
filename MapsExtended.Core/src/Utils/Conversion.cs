using UnityEngine;

namespace MapsExt.Utils
{
	public static class ConversionUtils
	{
		private const float UnitRatio = 40f / 1080f;

		public static Vector2 ScreenToWorldUnits(Vector2 p)
		{
			return new Vector2(p.x * UnitRatio, p.y * UnitRatio);
		}

		public static Vector2 WorldToScreenUnits(Vector2 p)
		{
			return new Vector2(p.x / UnitRatio, p.y / UnitRatio).Round(0);
		}
	}
}
