using System.Collections.Generic;

#pragma warning disable CS0649

namespace MapsExt.Compatibility.V0
{
	internal class CustomMap : IUpgradable
	{

		public string id;
		public string name;

#pragma warning disable CS0618
		public List<MapsExt.MapObjects.MapObject> mapObjects;
#pragma warning restore CS0618

		public object Upgrade()
		{
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
					UnityEngine.Debug.LogWarning($"Could not load map object {mapObject}");
				}
			}

			return new MapsExt.CustomMap
			{
				id = this.id,
				name = this.name,
				mapObjects = list
			};
		}
	}
}

#pragma warning restore CS0649
