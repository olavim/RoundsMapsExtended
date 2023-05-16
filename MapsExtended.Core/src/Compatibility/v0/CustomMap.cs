using System;
using System.Collections.Generic;

#pragma warning disable CS0649

namespace MapsExt.Compatibility.V0
{
	[Obsolete("Deprecated")]
	public class CustomMap : IUpgradable
	{
		public string id;
		public string name;

		public List<MapsExt.MapObjects.MapObject> mapObjects;

		public object Upgrade()
		{
			if (this.mapObjects == null)
			{
				throw new Exception($"Could not load map: {this.name ?? "<unnamed>"} ({this.id})");
			}

			var list = new List<MapsExt.MapObjects.MapObjectData>();

			foreach (var mapObject in this.mapObjects)
			{
				if (mapObject is IUpgradable upgradeable)
				{
					list.Add((MapsExt.MapObjects.MapObjectData) upgradeable.Upgrade());
				}
				else if (mapObject is MapsExt.MapObjects.MapObjectData data)
				{
					list.Add(data);
				}
				else
				{
					UnityEngine.Debug.LogError($"Could not load map object {mapObject}");
				}
			}

			return new MapsExt.CustomMap(this.id, this.name, MapsExtended.ModVersion, list.ToArray());
		}
	}
}

#pragma warning restore CS0649
