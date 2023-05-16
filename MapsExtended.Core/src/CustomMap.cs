using MapsExt.MapObjects;
using UnityEngine;

namespace MapsExt
{
	public class CustomMap
	{
		[SerializeField] private readonly string _id;
		[SerializeField] private readonly string _name;
		[SerializeField] private readonly string _version;
		[SerializeField] private readonly MapObjectData[] _mapObjects;

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
