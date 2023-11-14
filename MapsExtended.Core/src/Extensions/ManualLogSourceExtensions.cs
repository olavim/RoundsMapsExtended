using BepInEx.Logging;
using System;

namespace MapsExt
{
	public static class ManualLogSourceExtensions
	{
		public static void LogException(this ManualLogSource source, Exception ex)
		{
			string msg = ex.Message + "\n" + ex.StackTrace;
			while (ex.InnerException != null)
			{
				ex = ex.InnerException;
				msg = ex.StackTrace + "\n" + msg;
			}
			MapsExtended.Log.LogError(msg);
		}
	}
}
