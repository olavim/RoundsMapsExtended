using System;

namespace MapsExt
{
	[Obsolete("Deprecated")]
	[AttributeUsage(AttributeTargets.Class, Inherited = false)]
	public class MapObjectSpec : Attribute
	{
		public string name;
		public Type dataType;

		public MapObjectSpec(Type dataType)
		{
			this.name = dataType.Name;
			this.dataType = dataType;
		}

		public MapObjectSpec(string name, Type dataType)
		{
			this.name = name;
			this.dataType = dataType;
		}
	}

	[Obsolete("Deprecated")]
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class MapObjectSerializer : Attribute { }

	[Obsolete("Deprecated")]
	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class MapObjectDeserializer : Attribute { }

	[Obsolete("Deprecated")]
	[AttributeUsage(AttributeTargets.Property, Inherited = false)]
	public class MapObjectPrefab : Attribute { }
}
