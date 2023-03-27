using MapsExt.Compatibility.V0.MapObjects;
using System.Collections.Generic;

namespace MapsExt.Compatibility.V0
{
	public class CustomMap : IUpgradable
	{
		public string id;
		public string name;
		public List<MapObject> mapObjects;

		public object Upgrade()
		{
			return new MapsExt.CustomMap
			{
				id = this.id,
				name = this.name,
				mapObjects = this.mapObjects.ConvertAll(mapObject => (MapsExt.MapObjects.MapObjectData) mapObject.Upgrade())
			};
		}
	}
}
