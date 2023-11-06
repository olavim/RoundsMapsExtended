using System;

namespace MapsExt.Properties
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class PropertySerializerAttribute : Attribute
	{
		public Type PropertyType { get; }

		public PropertySerializerAttribute(Type propertyType)
		{
			this.PropertyType = propertyType;
		}
	}
}
