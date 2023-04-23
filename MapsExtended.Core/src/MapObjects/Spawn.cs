using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SpawnData : MapObjectData
	{
		private int _id;
		private int _teamId;
		private Vector2 _position;

		public SpawnIDProperty Id
		{
			get => new(this._id, this._teamId);
			set
			{
				this._id = value.Id;
				this._teamId = value.TeamId;
			}
		}
		public PositionProperty Position { get => this._position; set => this._position = value; }

		public SpawnData()
		{
			this.Id = new();
			this.Position = new();
		}
	}

	[MapObject(typeof(SpawnData))]
	public class Spawn : IMapObject
	{
		public virtual GameObject Prefab => NetworkedMapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
