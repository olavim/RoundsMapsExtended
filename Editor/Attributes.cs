using System;

namespace MapsExt.Editor
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MapsExtendedEditorMapObject : Attribute
	{
		public Type dataType;
		public string label;
		public string category;

		public MapsExtendedEditorMapObject(Type dataType, string label)
		{
			this.dataType = dataType;
			this.label = label;
			this.category = null;
		}

		public MapsExtendedEditorMapObject(Type dataType, string label, string category)
		{
			this.dataType = dataType;
			this.label = label;
			this.category = category;
		}
	}
}
