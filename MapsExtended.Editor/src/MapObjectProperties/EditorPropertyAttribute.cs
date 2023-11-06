using System;

namespace MapsExt.Editor.Properties
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class EditorPropertySerializerAttribute : Attribute
	{
		public Type PropertyType { get; }

		public EditorPropertySerializerAttribute(Type propertyType)
		{
			this.PropertyType = propertyType;
		}
	}
}
