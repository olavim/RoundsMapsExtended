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
		public List<MapObjectData> mapObjects;
	}
}
