using System;

namespace MapsExt.Editor
{
	[Obsolete("Deprecated")]
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

	[Obsolete("Deprecated")]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
	public class EditorMapObjectSerializer : Attribute { }

	[Obsolete("Deprecated")]
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property, Inherited = false)]
	public class EditorMapObjectDeserializer : Attribute { }

	[Obsolete("Deprecated")]
	[AttributeUsage(AttributeTargets.Property, Inherited = false)]
	public class EditorMapObjectPrefab : Attribute { }
}
