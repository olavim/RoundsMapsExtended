using UnityEngine;

namespace MapsExt
{
	public static class CameraExtensions
	{
		private const float Multi = 40f / 1080f;

		public static Vector3 ScreenToWorldUnits(this Camera camera, Vector3 p)
		{
			return new Vector3(p.x * Multi, p.y * Multi, p.z);
		}

		public static Vector3 WorldToScreenUnits(this Camera camera, Vector3 p)
		{
			return new Vector3(p.x * Multi, p.y * Multi, p.z);
		}
	}
}
