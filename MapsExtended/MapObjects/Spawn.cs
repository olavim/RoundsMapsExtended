using MapsExt.MapObjects.Properties;
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
			get => new SpawnIDProperty(this._id, this._teamId);
			set
			{
				this._id = value.Id;
				this._teamId = value.TeamId;
			}
		}
		public PositionProperty Position { get => this._position; set => this._position = value; }

		public SpawnData()
		{
			this.Id = new SpawnIDProperty();
			this.Position = new PositionProperty();
		}
	}

	[MapObject]
	public class Spawn : IMapObject<SpawnData>
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");

		public virtual void OnInstantiate(GameObject instance) { }
	}
}
