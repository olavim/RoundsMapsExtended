using System;

namespace MapsExt.MapObjects
{
	public abstract class MapObjectData
	{
		public string mapObjectId = Guid.NewGuid().ToString();
		public bool active = true;

		public override string ToString()
		{
			return $"MapObject ({this.GetType()})\nid: {this.mapObjectId}";
		}
	}
}
