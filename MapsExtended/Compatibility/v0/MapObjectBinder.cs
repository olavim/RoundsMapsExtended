using Sirenix.Serialization;
using System;

namespace MapsExt.Compatibility.V0.MapObjects
{
	internal class V0MapObjectBinder : TwoWaySerializationBinder
	{
		private static string GetV0TypeName(string typeName)
		{
			if (typeName.Contains("MapsExt.MapObjects.MapObject"))
			{
				return typeName;
			}

			return typeName.Replace("MapsExt.", "MapsExt.Compatibility.V0.");
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
