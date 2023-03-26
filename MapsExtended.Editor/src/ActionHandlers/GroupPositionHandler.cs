using MapsExt.MapObjects.Properties;
using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupActionHandler(typeof(PositionHandler))]
	public class GroupPositionHandler : PositionHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> gameObjects;

		private readonly Dictionary<GameObject, Vector2> localPositions = new Dictionary<GameObject, Vector2>();

		protected virtual void Awake()
		{
			foreach (var obj in this.gameObjects)
			{
				this.localPositions[obj] = obj.GetHandlerValue<PositionProperty>() - (Vector2) this.transform.position;
			}
		}

		public override void Move(PositionProperty delta)
		{
			this.SetValue((Vector2) this.transform.position + delta);
		}

		public override void SetValue(PositionProperty position)
		{
			foreach (var obj in this.gameObjects)
			{
				obj.SetHandlerValue<PositionProperty>(position + this.localPositions[obj]);
			}
			this.transform.position = position;
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this.gameObjects = gameObjects;
		}
	}
}
