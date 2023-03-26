using System;

namespace MapsExt.Editor.MapObjects
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class EditorMapObjectAttribute : Attribute
	{
		public string label;
		public string category;

		public EditorMapObjectAttribute(string label) : this(label, null) { }

		public EditorMapObjectAttribute(string label, string category)
		{
			this.label = label;
			this.category = category;
		}
	}
}
