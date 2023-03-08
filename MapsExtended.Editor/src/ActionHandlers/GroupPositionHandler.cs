using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupMapObjectActionHandler(typeof(PositionHandler))]
	public class GroupPositionHandler : PositionHandler, IGroupMapObjectActionHandler
	{
		private IEnumerable<GameObject> gameObjects;

		private readonly Dictionary<GameObject, Vector3> localPositions = new Dictionary<GameObject, Vector3>();

		protected virtual void Awake()
		{
			foreach (var obj in this.gameObjects)
			{
				this.localPositions[obj] = (Vector2) (obj.GetComponent<PositionHandler>().GetPosition() - this.transform.position);
			}
		}

		public override void Move(Vector3 delta)
		{
			this.SetPosition(this.transform.position + delta);
		}

		public override void SetPosition(Vector3 position)
		{
			foreach (var obj in this.gameObjects)
			{
				obj.GetComponent<PositionHandler>().SetPosition(position + this.localPositions[obj]);
			}
			this.transform.position = position;
		}

		public void Initialize(IEnumerable<GameObject> gameObjects)
		{
			this.gameObjects = gameObjects;
		}
	}
}
