using System.Collections.Generic;
using UnityEngine;

namespace MapsExt.Editor.ActionHandlers
{
	[GroupMapObjectActionHandler(typeof(PositionHandler))]
	public class GroupPositionHandler : PositionHandler, IGroupMapObjectActionHandler
	{
		public IEnumerable<GameObject> GameObjects { private get; set; }

		private readonly Dictionary<GameObject, Vector3> localPositions = new Dictionary<GameObject, Vector3>();

		private void Start()
		{
			foreach (var obj in this.GameObjects)
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
			foreach (var obj in this.GameObjects)
			{
				obj.GetComponent<PositionHandler>().SetPosition(position + this.localPositions[obj]);
			}
			this.transform.position = position;
		}
	}
}
