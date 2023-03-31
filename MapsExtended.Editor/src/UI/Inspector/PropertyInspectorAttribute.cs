using System;

namespace MapsExt.Editor.UI
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class PropertyInspectorAttribute : Attribute
	{
		public Type PropertyType { get; }

		public PropertyInspectorAttribute(Type propertyType)
		{
			PropertyType = propertyType;
		}
	}
}
