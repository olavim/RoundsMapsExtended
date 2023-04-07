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
		private readonly Dictionary<GameObject, Vector2> _localPositions = new();

		public override void Move(PositionProperty delta)
		{
			this.SetValue(this.GetValue() + delta);
		}

		public override void SetValue(PositionProperty position)
		{
			this.transform.position = position;
			this.RefreshPositions();
		}

		public virtual void RefreshPositions()
		{
			foreach (var obj in this._gameObjects)
			{
				var newLocalPos = this.transform.rotation * this._localPositions[obj];
				obj.SetHandlerValue<PositionProperty>((this.transform.position + newLocalPos).Round(4));
			}
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this._gameObjects = gameObjects;

			var bounds = new Bounds(this._gameObjects.First().transform.position, Vector3.zero);
			foreach (var obj in this._gameObjects)
			{
				bounds.Encapsulate(obj.GetHandlerValue<PositionProperty>());
			}

			this.transform.position = bounds.center;

			foreach (var obj in this._gameObjects)
			{
				this._localPositions[obj] = obj.GetHandlerValue<PositionProperty>() - (PositionProperty) this.transform.position;
			}
		}
	}
}
