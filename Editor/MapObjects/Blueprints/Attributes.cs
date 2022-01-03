using System;

namespace MapsExt.Editor.MapObjects
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class EditorMapObjectBlueprint : Attribute
	{
		public string label;
		public string category;

		public EditorMapObjectBlueprint(string label) : this(label, null) { }

		public EditorMapObjectBlueprint(string label, string category)
		{
			this.label = label;
			this.category = category;
		}
	}
}
