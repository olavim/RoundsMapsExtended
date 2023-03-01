using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupMapObjectActionHandler(typeof(PositionHandler))]
	public class GroupPositionHandler : PositionHandler, IGroupMapObjectActionHandler
	{
		private GameObject[] gameObjects;

		public override void Move(Vector3 delta)
		{
			foreach (var obj in this.gameObjects)
			{
				obj.GetComponent<PositionHandler>().Move(delta);
			}
			this.transform.position += delta;
		}

		public override void SetPosition(Vector3 position)
		{
			this.Move(position - this.transform.position);
		}

		public void SetHandlers(IEnumerable<GameObject> gameObjects)
		{
			this.gameObjects = gameObjects.ToArray();
		}
	}
}
