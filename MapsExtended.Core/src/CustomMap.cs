using MapsExt.Compatibility;
using MapsExt.MapObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt
{
	public class CustomMap : ISerializationCallbackReceiver
	{
		[SerializeField] private readonly string _id;
		[SerializeField] private readonly string _name;
		[SerializeField] private readonly string _version;
		[SerializeField] private object[] _mapObjects;

		private MapObjectData[] _typedMapObjects;

		public string Id => this._id;
		public string Name => this._name;
		public string Version => this._version;
		public MapObjectData[] MapObjects => this._typedMapObjects;

		public CustomMap(string id, string name, string version, MapObjectData[] mapObjects)
		{
			this._id = id;
			this._name = name;
			this._typedMapObjects = mapObjects;
			this._version = version;
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			this._mapObjects = this._typedMapObjects.Cast<object>().ToArray();
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (this._mapObjects == null)
			{
				throw new System.Exception($"Could not load map: {this.Name ?? "<unnamed>"} ({this.Id})");
			}

			var list = new List<MapObjectData>();

			foreach (var mapObject in this._mapObjects)
			{
				if (mapObject is IUpgradable<MapObjectData> upgradeable)
				{
					list.Add(upgradeable.Upgrade());
				}
				else if (mapObject is MapObjectData data)
				{
					list.Add(data);
				}
				else
				{
					throw new System.Exception($"Could not load map: {this.Name ?? "<unnamed>"} ({this.Id}) (map object index: {list.Count})");
				}
			}

			this._typedMapObjects = list.ToArray();
		}
	}
}
