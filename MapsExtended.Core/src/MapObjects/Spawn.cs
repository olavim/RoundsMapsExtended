using MapsExt.Properties;
using UnityEngine;

namespace MapsExt.MapObjects
{
	public class SpawnData : MapObjectData
	{
		[SerializeField] private int _spawnId;
		[SerializeField] private int _spawnTeamId;
		[SerializeField] private Vector2 _position;

		public SpawnIDProperty Id
		{
			get => new(this._spawnId, this._spawnTeamId);
			set
			{
				this._spawnId = value.Id;
				this._spawnTeamId = value.TeamId;
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
	public class Spawn : IMapObject, IMapObjectDataWriteCallbackReceiver
	{
		public virtual GameObject Prefab => MapObjectManager.LoadCustomAsset<GameObject>("Spawn Point");

		public virtual void OnInstantiate(GameObject instance) { }

		public virtual void OnDataWrite(GameObject instance, MapObjectData data)
		{
			instance.GetComponent<SpawnPoint>().localStartPos = data.GetProperty<PositionProperty>();
		}
	}
}
