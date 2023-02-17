using System;
using System.Linq;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public abstract class MapObjectData
	{
		public string mapObjectId = Guid.NewGuid().ToString();
		public bool active = true;

		public MapObjectInstance FindInstance(GameObject container)
		{
			return container
				.GetComponentsInChildren<MapObjectInstance>(true)
				.FirstOrDefault(obj => obj.mapObjectId == this.mapObjectId);
		}

		public override string ToString()
		{
			return $"MapObject ({this.GetType()})\nid: {this.mapObjectId}";
		}
	}
}
