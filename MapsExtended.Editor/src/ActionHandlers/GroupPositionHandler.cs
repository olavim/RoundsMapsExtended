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
				obj.GetComponent<PositionHandler>().SetValue((this.transform.position + newLocalPos).Round(4));
			}
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this._gameObjects = gameObjects;

			var bounds = new Bounds(this._gameObjects.First().GetComponent<PositionHandler>().GetValue(), Vector3.zero);
			foreach (var obj in this._gameObjects)
			{
				bounds.Encapsulate(obj.GetComponent<PositionHandler>().GetValue());
			}

			this.transform.position = bounds.center;

			foreach (var obj in this._gameObjects)
			{
				this._localPositions[obj] = obj.GetComponent<PositionHandler>().GetValue() - this.GetValue();
			}
		}
	}
}
