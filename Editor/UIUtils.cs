using UnityEngine;

namespace MapsExt.Editor
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
            var min = MainCam.instance.cam.WorldToScreenPoint(rect.min);
            var max = MainCam.instance.cam.WorldToScreenPoint(rect.max);
            return new Rect(min.x, Screen.height - max.y, max.x - min.x, max.y - min.y);
        }

        public static Rect ScreenToWorldRect(Rect rect)
        {
            var min = MainCam.instance.cam.ScreenToWorldPoint(rect.min);
            var max = MainCam.instance.cam.ScreenToWorldPoint(rect.max);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }

        public static Rect WorldToScreenRect(Rect rect)
        {
            var min = MainCam.instance.cam.WorldToScreenPoint(rect.min);
            var max = MainCam.instance.cam.WorldToScreenPoint(rect.max);
            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }
}
