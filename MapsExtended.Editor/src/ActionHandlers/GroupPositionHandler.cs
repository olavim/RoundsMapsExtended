using MapsExt.Properties;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(PositionHandler), typeof(SelectionHandler))]
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

			var boundsArr = gameObjects.Select(obj => obj.GetComponent<SelectionHandler>().GetBounds()).ToArray();
			var bounds = boundsArr[0];
			for (var i = 1; i < boundsArr.Length; i++)
			{
				bounds.Encapsulate(boundsArr[i]);
			}

			this.transform.position = bounds.center;

			foreach (var obj in this._gameObjects)
			{
				this._localPositions[obj] = obj.GetComponent<PositionHandler>().GetValue() - (PositionProperty) bounds.center;
			}
		}
	}
}
