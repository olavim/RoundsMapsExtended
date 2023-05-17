using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0649

namespace MapsExt.Compatibility.V0
{
	[Obsolete("Deprecated")]
	public class CustomMap : IUpgradable<MapsExt.CustomMap>
	{
		public string id;
		public string name;
		public object[] mapObjects;

		public CustomMap(string id, string name, object[] mapObjects)
		{
			this.id = id;
			this.name = name;
			this.mapObjects = mapObjects.Cast<object>().ToArray();
		}

		public MapsExt.CustomMap Upgrade()
		{
			if (this.mapObjects == null)
			{
				throw new Exception($"Could not load map: {this.name ?? "<unnamed>"} ({this.id})");
			}

			var list = new List<MapsExt.MapObjects.MapObjectData>();

			foreach (var mapObject in this.mapObjects)
			{
				if (mapObject is IUpgradable<MapsExt.MapObjects.MapObjectData> upgradeable)
				{
					list.Add(upgradeable.Upgrade());
				}
				else if (mapObject is MapsExt.MapObjects.MapObjectData data)
				{
					list.Add(data);
				}
				else
				{
					throw new Exception($"Could not load map: {this.name ?? "<unnamed>"} ({this.id})");
				}
			}

			return new MapsExt.CustomMap(this.id, this.name, MapsExtended.ModVersion, list.ToArray());
		}
	}
}

#pragma warning restore CS0649
