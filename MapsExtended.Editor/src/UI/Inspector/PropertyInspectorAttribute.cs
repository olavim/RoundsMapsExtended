using System;

namespace MapsExt.Editor.UI
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public sealed class InspectorElementAttribute : Attribute
	{
		public Type PropertyType { get; }

		public InspectorElementAttribute(Type propertyType)
		{
			PropertyType = propertyType;
		}
	}
}
