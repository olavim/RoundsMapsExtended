using System;

namespace MapsExt.Editor.MapObjects
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class EditorMapObject : Attribute
	{
		public string label;
		public string category;

		public EditorMapObject(string label) : this(label, null) { }

		public EditorMapObject(string label, string category)
		{
			this.label = label;
			this.category = category;
		}
	}
}
