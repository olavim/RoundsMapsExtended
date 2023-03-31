using System;

namespace MapsExt.MapObjects.Properties
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class PropertySerializerAttribute : Attribute
	{
		public Type PropertyType { get; }

		public PropertySerializerAttribute(Type propertyType)
		{
			PropertyType = propertyType;
		}
	}
}
