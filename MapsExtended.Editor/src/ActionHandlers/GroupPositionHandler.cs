using MapsExt.Properties;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(PositionHandler))]
	public class GroupPositionHandler : PositionHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> _gameObjects;
		private readonly Dictionary<GameObject, Vector2> _localPositions = new();

		protected virtual void Awake()
		{
			foreach (var obj in this._gameObjects)
			{
				this._localPositions[obj] = obj.GetHandlerValue<PositionProperty>() - (Vector2) this.transform.position;
			}
		}

		public override void Move(PositionProperty delta)
		{
			this.SetValue((Vector2) this.transform.position + delta);
		}

		public override void SetValue(PositionProperty position)
		{
			foreach (var obj in this._gameObjects)
			{
				obj.SetHandlerValue<PositionProperty>(position + this._localPositions[obj]);
			}
			this.transform.position = position;
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this._gameObjects = gameObjects;
		}
	}
}
