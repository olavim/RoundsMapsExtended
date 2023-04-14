using MapsExt.MapObjects;
using System;

namespace MapsExt
{
	[Serializable]
	public class CustomMap
	{
		private readonly string _id;
		private readonly string _name;
		private readonly string _version;
		private readonly MapObjectData[] _mapObjects;

		public string Id => this._id;
		public string Name => this._name;
		public string Version => this._version;
		public MapObjectData[] MapObjects => this._mapObjects;

		public CustomMap(string id, string name, string version, MapObjectData[] mapObjects)
		{
			this._id = id;
			this._name = name;
			this._mapObjects = mapObjects;
			this._version = version;
		}
	}
}
