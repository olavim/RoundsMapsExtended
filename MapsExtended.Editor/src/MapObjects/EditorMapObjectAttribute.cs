using System;

namespace MapsExt.Editor.MapObjects
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class EditorMapObjectAttribute : Attribute
	{
		public Type DataType { get; }
		public string Label { get; }
		public string Category { get; init; }

		public EditorMapObjectAttribute(Type dataType, string label)
		{
			this.DataType = dataType;
			this.Label = label;
		}
	}
}
