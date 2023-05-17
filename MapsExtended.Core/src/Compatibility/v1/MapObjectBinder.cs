using Sirenix.Serialization;
using System;

namespace MapsExt.Compatibility.V1.MapObjects
{
	internal class V1MapObjectBinder : TwoWaySerializationBinder
	{
		private static string GetV1TypeName(string typeName)
		{
			if (typeName == "MapsExt.MapObjects.MapObjectData[], MapsExtended")
			{
				return "System.Object[], mscorlib";
			}

			return typeName;
		}

		public override bool ContainsType(string typeName)
		{
			return TwoWaySerializationBinder.Default.ContainsType(GetV1TypeName(typeName));
		}

		public override Type BindToType(string typeName, DebugContext debugContext = null)
		{
			return TwoWaySerializationBinder.Default.BindToType(GetV1TypeName(typeName), debugContext);
		}

		public override string BindToName(Type type, DebugContext debugContext = null)
			=> throw new NotImplementedException();
	}
}
