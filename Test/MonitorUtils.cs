using MapsExt.Test.Bindings;
using System;
using System.Diagnostics;
using UnityEngine;

namespace MapsExt.Test
{
	public static class MonitorUtils
	{
		static IntPtr windowHandle = IntPtr.Zero;

		static IntPtr GetWindow()
		{
			if (MonitorUtils.windowHandle == IntPtr.Zero)
			{
				var processName = Process.GetCurrentProcess().ProcessName;
				MonitorUtils.windowHandle = WindowsBindings.FindWindow(null, processName);
			}

			return MonitorUtils.windowHandle;
		}

		public static Vector2Int ScreenToMonitorPoint(Vector2Int point)
		{
			var topLeftPoint = new Vector2Int(0, 0);
			WindowsBindings.ClientToScreen(MonitorUtils.GetWindow(), ref topLeftPoint);
			return new Vector2Int(topLeftPoint.x + point.x, topLeftPoint.y + point.y);
		}
	}
}
