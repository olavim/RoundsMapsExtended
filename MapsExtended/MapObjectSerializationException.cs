using System;

namespace MapsExt
{
	public class MapObjectSerializationException : Exception
	{
		public MapObjectSerializationException() { }
		public MapObjectSerializationException(string message) : base(message) { }
		public MapObjectSerializationException(string message, Exception innerException) : base(message, innerException) { }
	}
}
