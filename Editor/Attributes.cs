using System;

namespace MapsExt.Editor
{
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class EditorMapObjectSpec : Attribute
	{
		public Type dataType;
		public string label;
		public string category;

		public EditorMapObjectSpec(Type dataType, string label)
		{
			this.dataType = dataType;
			this.label = label;
			this.category = null;
		}

		public EditorMapObjectSpec(Type dataType, string label, string category)
		{
			this.dataType = dataType;
			this.label = label;
			this.category = category;
		}
	}

	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class EditorInspectorSpec : Attribute
	{
		public Type inspectorSpecType;

		public EditorInspectorSpec(Type inspectorSpecType)
		{
			this.inspectorSpecType = inspectorSpecType;
		}
	}

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
	public class EditorMapObjectSerializer : Attribute { }

	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
	public class EditorMapObjectDeserializer : Attribute { }

	[AttributeUsage(AttributeTargets.Property, Inherited = false)]
	public class EditorMapObjectPrefab : Attribute { }
}
