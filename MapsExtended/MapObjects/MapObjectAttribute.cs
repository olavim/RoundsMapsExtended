using System;

namespace MapsExt.MapObjects
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class MapObjectAttribute : Attribute
	{
		public Type DataType { get; }

		public MapObjectAttribute(Type dataType)
		{
			DataType = dataType;
		}
	}
}
