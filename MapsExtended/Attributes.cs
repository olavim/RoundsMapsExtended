using System;

namespace MapsExt
{
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

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class MapObjectSerializer : Attribute { }

	[AttributeUsage(AttributeTargets.Method, Inherited = false)]
	public class MapObjectDeserializer : Attribute { }

	[AttributeUsage(AttributeTargets.Property, Inherited = false)]
	public class MapObjectPrefab : Attribute { }
}
