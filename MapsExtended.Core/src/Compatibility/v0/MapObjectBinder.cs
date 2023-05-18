using Sirenix.Serialization;
using System;

namespace MapsExt.Compatibility.V0.MapObjects
{
	internal class V0MapObjectBinder : TwoWaySerializationBinder
	{
		private static string GetV0TypeName(string typeName)
		{
			if (typeName == "System.Collections.Generic.List`1[[MapsExt.MapObjects.MapObject, MapsExtended]], mscorlib")
			{
				return "MapsExt.MapObjects.MapObjectData[], MapsExtended";
			}

			typeName = typeName.Replace("MapsExt.CustomMap", "MapsExt.Compatibility.V0.CustomMap");
			typeName = typeName.Replace("MapsExt.MapObjects", "MapsExt.Compatibility.V0.MapObjects");
			return typeName;
		}

		public override bool ContainsType(string typeName)
		{
			return TwoWaySerializationBinder.Default.ContainsType(GetV0TypeName(typeName));
		}

		public override Type BindToType(string typeName, DebugContext debugContext = null)
		{
			return TwoWaySerializationBinder.Default.BindToType(GetV0TypeName(typeName), debugContext);
		}

		public override string BindToName(Type type, DebugContext debugContext = null)
			=> throw new NotImplementedException();
	}
}
