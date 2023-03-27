using MapsExt.MapObjects;
using System;
using System.Collections.Generic;

namespace MapsExt
{
	[Serializable]
	public class CustomMap
	{
		public string id;
		public string name;
		public string version = MapsExtended.ModVersion;
		public List<MapObjectData> mapObjects;
	}
}
