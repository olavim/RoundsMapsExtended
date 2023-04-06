using MapsExt.Properties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(PositionHandler))]
	public class GroupPositionHandler : PositionHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> _gameObjects;
		private readonly Dictionary<GameObject, PositionProperty> _localPositions = new();

		protected override void Awake()
		{
			this.RefreshLocalPositions();
		}

		public override void Move(PositionProperty delta)
		{
			this.SetValue(this.GetValue() + delta);
		}

		public override void SetValue(PositionProperty position)
		{
			foreach (var obj in this._gameObjects)
			{
				obj.SetHandlerValue(position + this._localPositions[obj]);
			}
			this.transform.position = position;
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this._gameObjects = gameObjects;

			var pos = Vector2.zero;
			foreach (var obj in this._gameObjects)
			{
				pos += obj.GetHandlerValue<PositionProperty>().Value;
			}

			this.transform.position = pos / this._gameObjects.Count();
		}

		public void RefreshLocalPositions()
		{
			foreach (var obj in this._gameObjects)
			{
				this._localPositions[obj] = obj.GetHandlerValue<PositionProperty>() - (PositionProperty) this.transform.position;
			}
		}
	}
}
