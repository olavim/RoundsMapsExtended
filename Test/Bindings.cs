using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace MapsExt.Test.Bindings
{
	public class WindowsBindings
	{
		[Flags]
		public enum MouseEventFlags
		{
			LeftDown = 0x00000002,
			LeftUp = 0x00000004,
			MiddleDown = 0x00000020,
			MiddleUp = 0x00000040,
			Move = 0x00000001,
			Absolute = 0x00008000,
			RightDown = 0x00000008,
			RightUp = 0x00000010
		}

		[DllImport("user32.dll", EntryPoint = "SetCursorPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SetCursorPos(int x, int y);

		[DllImport("user32.dll", EntryPoint = "GetCursorPos")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetCursorPos(out Vector2Int lpMousePoint);

		[DllImport("user32.dll", EntryPoint = "mouse_event")]
		public static extern void SendMouseEvent(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

		[DllImport("user32.dll", EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindow(string className, string windowName);

		[DllImport("user32.dll", EntryPoint = "ClientToScreen")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ClientToScreen(IntPtr hwnd, ref Vector2Int point);
	}
}