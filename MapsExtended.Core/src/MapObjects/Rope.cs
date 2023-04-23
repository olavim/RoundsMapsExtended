using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class RopeData : MapObjectData
	{
		private Vector2 _pos1;
		private Vector2 _pos2;

		public RopePositionProperty Position
		{
			get => new(this._pos1, this._pos2);
			set { this._pos1 = value.StartPosition; this._pos2 = value.EndPosition; }
		}

		public RopeData()
		{
			this.Position = new();
		}
	}

	[MapObject(typeof(RopeData))]
	public class Rope : IMapObject
	{
		public virtual GameObject Prefab => NetworkedMapObjectManager.LoadCustomAsset<GameObject>("Rope");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
