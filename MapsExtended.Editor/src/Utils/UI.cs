using UnityEngine;

namespace MapsExt.Editor.Utils
{
	public static class UIUtils
	{
		public static Texture2D GetTexture(int width, int height, Color color)
		{
			var pixels = new Color[width * height];
			for (int i = 0; i < pixels.Length; ++i)
			{
				pixels[i] = color;
			}

			Texture2D result = new(width, height);
			result.SetPixels(pixels);
			result.Apply();

			return result;
		}

		public static Rect GUIToWorldRect(Rect rect)
		{
			var min = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(rect.min.x, Screen.height - rect.min.y));
			var max = MainCam.instance.cam.ScreenToWorldPoint(new Vector2(rect.max.x, Screen.height - rect.max.y));
			return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
		}
	}
}
