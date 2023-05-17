﻿using MapsExt.Compatibility;
using MapsExt.MapObjects;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt
{
	public class CustomMap : IUpgradable<CustomMap>
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

		public CustomMap Upgrade()
		{
			if (this.MapObjects == null)
			{
				throw new System.Exception($"Could not load map: {this.Name ?? "<unnamed>"} ({this.Id})");
			}

			var list = new List<MapObjectData>();

			foreach (var mapObject in this.MapObjects)
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
					UnityEngine.Debug.LogError($"Could not load map object {mapObject}");
				}
			}

			return new CustomMap(this.Id, this.Name, this.Version, list.ToArray());
		}
	}
}
