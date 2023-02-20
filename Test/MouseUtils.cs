using MapsExt.Test.Bindings;
using UnityEngine;

namespace MapsExt.Test
{
	public class MouseUtils
	{
		public static void SetCursorPosition(int x, int y)
		{
			WindowsBindings.SetCursorPos(x, y);
		}

		public static void SetCursorPosition(Vector2Int point)
		{
			WindowsBindings.SetCursorPos(point.x, point.y);
		}

		public static Vector2Int GetCursorPosition()
		{
			Vector2Int currentMousePoint;
			var gotPoint = WindowsBindings.GetCursorPos(out currentMousePoint);
			if (!gotPoint) { currentMousePoint = new Vector2Int(0, 0); }
			return currentMousePoint;
		}

		public static void MouseDown()
		{
			MouseUtils.MouseEvent(WindowsBindings.MouseEventFlags.LeftDown);
		}

		public static void MouseUp()
		{
			MouseUtils.MouseEvent(WindowsBindings.MouseEventFlags.LeftUp);
		}

		public static void MouseClick()
		{
			MouseUtils.MouseEvent(WindowsBindings.MouseEventFlags.LeftDown | WindowsBindings.MouseEventFlags.LeftUp);
		}

		public static void MouseEvent(WindowsBindings.MouseEventFlags value)
		{
			Vector2Int position = GetCursorPosition();
			WindowsBindings.SendMouseEvent((int) value, position.x, position.y, 0, 0);
		}
	}
}