using System;

namespace MapsExt.Tests
{
	internal class SerializationLogger : Sirenix.Serialization.ILogger
	{
		public void LogError(string error)
		{
			Surity.Debug.Log($"[ERROR] {error}");
		}

		public void LogException(Exception exception)
		{
			Surity.Debug.Log($"[EXCEPTION] {exception}");
		}

		public void LogWarning(string warning)
		{
			Surity.Debug.Log($"[WARNING] {warning}");
		}
	}
}
