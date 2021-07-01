using UnityEngine;

namespace MapEditor
{
    public static class GUIUtils
    {
        public static Texture2D GetTexture(int width, int height, Color color)
        {
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; ++i)
            {
                pixels[i] = color;
            }

            Texture2D result = new Texture2D(width, height);
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

        public static Rect WorldToGUIRect(Rect rect)
        {
            var min = MainCam.instance.cam.WorldToScreenPoint(new Vector2(rect.min.x, rect.min.y));
            var max = MainCam.instance.cam.WorldToScreenPoint(new Vector2(rect.max.x, rect.max.y));
            return new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
        }
    }
}
